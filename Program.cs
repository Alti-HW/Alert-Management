using System.Diagnostics;
using Alert_Management.Data;
using Alert_Management.Infterfaces;
using Alert_Management.Services;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using WebSocketManager = Alert_Management.Services.WebSocketManager;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Run executables when the application starts
//RunExternalProcesses();
//void RunExternalProcesses()
//{
//    string rootPath = AppContext.BaseDirectory;

//    string exe1 = "./Prometheus_Config/Prometheus/prometheus.exe";
//    string exe2 = "./Prometheus_Config/AlertManager/alertmanager.exe";
//    // Run Prometheus on port 9090
//    RunProcess(exe1, "--config.file=prometheus.yml --web.listen-address=:9090");

//    // Run Alertmanager on port 9093
//    RunProcess(exe2, "--config.file=alertmanager.yml --web.listen-address=:9093");
//}

//void RunProcess(string filePath, string arguments)
//{
//    string fullPath = Path.GetFullPath(filePath); // Convert to absolute path

//    if (File.Exists(fullPath))
//    {
//        Process.Start(new ProcessStartInfo
//        {
//            FileName = fullPath,  // Use absolute path
//            Arguments = arguments,
//            UseShellExecute = true,
//            CreateNoWindow = false
//        });

//        Console.WriteLine($"Started: {fullPath} with arguments: {arguments}");
//    }
//    else
//    {
//        Console.WriteLine($"File not found: {fullPath}");
//    }
//}



builder.Services.AddScoped<WebSocketHandler>();
// Add WebSocket support
builder.Services.AddScoped<WebSocketManager>();
// Configure PostgreSQL for Alert Database
builder.Services.AddDbContext<AlertDbContext >(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AlertDBConnection")));

// Configure PostgreSQL for Monitoring Database (Energy Consumption Data)
builder.Services.AddDbContext<MonitoringDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MonitoringDBConnection")));

// Register services
builder.Services.AddScoped<IAlertRuleService, PrometheusAlertRuleService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddHostedService<EnergyMetricsService>();

// Register Background Service for Alert Processing
//builder.Services.AddHostedService<AlertBackgroundService>();

// Add Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();


// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Read the URL from appsettings.json
var kestrelSection = builder.Configuration.GetSection("Kestrel:Endpoints:Http:Url");
var url = kestrelSection.Value ?? "http://localhost:5000"; // Default if not set

builder.WebHost.UseUrls(url); // Correct way to set URL
var app = builder.Build();

// Enable CORS
app.UseCors("AllowAll");

//// Enable Swagger in Development
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

// Enable Prometheus Metrics
app.UseMetricServer();
// Enable WebSockets
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
};
app.UseWebSockets(webSocketOptions);
// Add middleware for WebSockets


app.UseRouting();  // ✅ Add this before UseEndpoints()
app.UseEndpoints(endpoints =>
{
    endpoints.MapMetrics(); // Exposes Prometheus metrics at /metrics
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
