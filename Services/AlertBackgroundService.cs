using System;
using System.Threading;
using System.Threading.Tasks;
using Alert_Management.Infterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Alert_Management.Services
{
    public class AlertBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<AlertBackgroundService> _logger;

        public AlertBackgroundService(IServiceScopeFactory serviceScopeFactory, ILogger<AlertBackgroundService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Alert background service is running...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var alertService = scope.ServiceProvider.GetRequiredService<IAlertProcessingService>();

                    await alertService.ProcessAlertsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing alerts.");
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // Run every 1 minute
            }
        }
    }
}
