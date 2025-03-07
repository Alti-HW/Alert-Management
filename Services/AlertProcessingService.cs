using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Alert_Management.Data;
using Alert_Management.DTOs;
using Alert_Management.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Alert_Management.Services
{
    public class AlertProcessingService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string _monitoringDbConnectionString;
        private readonly ILogger<AlertProcessingService> _logger;

        public AlertProcessingService(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<AlertProcessingService> logger)
        {
            _scopeFactory = scopeFactory;
            _monitoringDbConnectionString = configuration.GetConnectionString("MonitoringDBConnection");
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await using var conn = new NpgsqlConnection(_monitoringDbConnectionString);
            await conn.OpenAsync(stoppingToken);
            await using var cmd = new NpgsqlCommand("LISTEN energy_inserted;", conn);
            await cmd.ExecuteNonQueryAsync(stoppingToken);

            _logger.LogInformation("Listening for energy data insertions...");

            conn.Notification += async (sender, e) => await HandleNotification(e.Payload);

            while (!stoppingToken.IsCancellationRequested)
            {
                await conn.WaitAsync(stoppingToken);
            }
        }

        private async Task HandleNotification(string payload)
        {
            try
            {
                _logger.LogInformation($"Received payload: {payload}");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // ✅ Deserialize JSON into DTO
                var dto = JsonSerializer.Deserialize<EnergyConsumptionDto>(payload, options);
                if (dto == null)
                {
                    _logger.LogWarning("Received null or malformed JSON.");
                    return;
                }

                // ✅ Convert DTO to EnergyConsumption Model
                var newEntry = new EnergyConsumption
                {
                    ConsumptionId = dto.ConsumptionId,
                    FloorId = dto.FloorId,
                    Timestamp = dto.Timestamp,
                    EnergyConsumedKwh = dto.EnergyConsumedKwh,
                    PeakLoadKw = dto.PeakLoadKw,
                    AvgTemperatureC = dto.AvgTemperatureC,
                    Co2EmissionsKg = dto.Co2EmissionsKg
                };

                using var scope = _scopeFactory.CreateScope();
                var alertDbContext = scope.ServiceProvider.GetRequiredService<AlertDbContext>();

                var allRules = await alertDbContext.AlertRules
                    .Where(r => r.IsEnabled)
                    .ToListAsync();

                var matchingRules = allRules
                    .Where(r => EvaluateCondition(r, newEntry)) // ✅ Now dynamically checking based on rule metric
                    .ToList();

                if (!matchingRules.Any())
                {
                    _logger.LogWarning("No matching alert rules found.");
                    return;
                }

                foreach (var rule in matchingRules)
                {
                    // ✅ Get the actual value dynamically based on the metric
                    decimal actualValue = rule.Metric switch
                    {
                        "energy_consumed_kwh" => newEntry.EnergyConsumedKwh,
                        "peak_load_kw" => newEntry.PeakLoadKw,
                        "avg_temperature_c" => newEntry.AvgTemperatureC,
                        "co2_emissions_kg" => newEntry.Co2EmissionsKg,
                        _ => throw new Exception($"Unknown metric: {rule.Metric}")
                    };

                    // ✅ Updated alert message to include actual value
                    string message = $"Alert triggered for {rule.Name}: {rule.Metric} exceeded threshold {rule.Threshold} (Actual: {actualValue})";


                    var alert = new Alert
                    {
                        ApplicationId = rule.ApplicationId,
                        RuleId = rule.Id,
                        Message = message,
                        Status = "Active",
                        TriggeredAt = DateTime.UtcNow
                    };

                    alertDbContext.Alerts.Add(alert);
                    _logger.LogInformation($"ALERT TRIGGERED: {message}");
                }

                await alertDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing alert: {ex.Message}");
            }
        }

        private bool EvaluateCondition(AlertRule rule, EnergyConsumption data)
        {
            decimal valueToCheck = rule.Metric switch
            {
                "energy_consumed_kwh" => data.EnergyConsumedKwh,
                "peak_load_kw" => data.PeakLoadKw,
                "avg_temperature_c" => data.AvgTemperatureC,
                "co2_emissions_kg" => data.Co2EmissionsKg,
                _ => throw new Exception($"Unknown metric: {rule.Metric}")
            };

            return rule.Expression switch
            {
                ">" => valueToCheck > rule.Threshold,
                "<" => valueToCheck < rule.Threshold,
                ">=" => valueToCheck >= rule.Threshold,
                "<=" => valueToCheck <= rule.Threshold,
                "==" => valueToCheck == rule.Threshold,
                _ => throw new Exception($"Invalid expression: {rule.Expression}")
            };
        }

    }
}
