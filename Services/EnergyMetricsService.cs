using Npgsql;
using Microsoft.Extensions.Hosting;
using Prometheus;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class EnergyMetricsService : BackgroundService
{
    private readonly string _connectionString = "Host=pg-1c707a6d-basamdileepkumar-9fe8.d.aivencloud.com;Port=26900;Username=avnadmin;Password=AVNS__6UJ9rVOAAkzZi6LUJ6;Database=EMS_HoneyWell;SSL Mode=Require";
    private readonly Dictionary<string, Gauge> _gauges = new();

    public EnergyMetricsService()
    {
        RegisterMetrics("energy_consumption", new[] { "energy_consumed_kwh", "peak_load_kw", "avg_temperature_c", "co2_emissions_kg" });
    }

    private void RegisterMetrics(string tableName, string[] columns)
    {
        foreach (var col in columns)
        {
            _gauges[$"{tableName}_{col}"] = Metrics.CreateGauge($"{tableName}_{col}", $"Latest {col} from {tableName}", new[] { "floor_id" });
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(stoppingToken);

        using var cmd = new NpgsqlCommand("LISTEN data_update;", conn);
        await cmd.ExecuteNonQueryAsync(stoppingToken);

        conn.Notification += async (o, e) => await HandleTableUpdate(e.Payload);

        while (!stoppingToken.IsCancellationRequested)
        {
            await conn.WaitAsync(stoppingToken);
        }
    }

    private async Task HandleTableUpdate(string payload)
    {
        Console.WriteLine($"Data change detected: {payload}");

        // Split payload into table name and floor_id
        string[] parts = payload.Split(' ');

        if (parts.Length < 2)
        {
            Console.WriteLine("Invalid payload format.");
            return;
        }

        string tableName = parts[0].Trim().ToLower();
        string floor_id = parts[1].Trim();  // Ensure it's extracted properly

        Console.WriteLine($"Normalized tableName: '{tableName}', Floor ID: '{floor_id}'");

        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        // Query only for the updated consumption_id
        string query = tableName switch
        {
            "energy_consumption" => "SELECT floor_id, energy_consumed_kwh, peak_load_kw, avg_temperature_c, co2_emissions_kg FROM energy_consumption WHERE floor_id = @floor_id",
            _ => null
        };

        if (query == null)
        {
            Console.WriteLine("Query is null, skipping processing.");
            return;
        }

        using var cmd = new NpgsqlCommand(query, conn);


        // Convert floor_id to an integer before passing it to the query
        if (!int.TryParse(floor_id, out int floor_idInt))
        {
            Console.WriteLine($"Invalid floor_id: {floor_id}");
            return;
        }

        cmd.Parameters.AddWithValue("@floor_id", floor_idInt);


        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            string id = reader["floor_id"].ToString();

            foreach (var key in _gauges.Keys)
            {
                if (key.StartsWith($"{tableName}_"))
                {
                    string column = key.Replace($"{tableName}_", "");

                    if (!reader.IsDBNull(reader.GetOrdinal(column)))
                    {
                        double value = reader.GetDouble(reader.GetOrdinal(column));
                        _gauges[key].WithLabels(floor_id).Set(value);
                        Console.WriteLine($"Updated metric: {key} for ID {id} -> {value}");
                    }
                    else
                    {
                        Console.WriteLine($"Skipping null value for {column} in {tableName}");
                    }
                }
            }
        }
    }

}
