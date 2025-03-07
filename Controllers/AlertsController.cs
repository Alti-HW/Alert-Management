using Microsoft.AspNetCore.Mvc;
using Alert_Management.Data;
using Alert_Management.Models;
using Alert_Management.DTOs;
using Alert_Management.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Alert_Management.Infterfaces;
using WebSocketManager = Alert_Management.Services.WebSocketManager;
using System.Net.WebSockets;

namespace Alert_Management.Controllers
{
    [ApiController]
    [Route("api/alerts")]
    public class AlertsController : ControllerBase
    {
        private readonly AlertDbContext _context;
        private readonly IAlertService _alertService;
        private readonly WebSocketManager _webSocketManager;

        public AlertsController(AlertDbContext context, IAlertService alertService, WebSocketManager webSocketManager)
        {
            _context = context;
            _alertService = alertService;
            _webSocketManager = webSocketManager;
        }

        [HttpGet("ws/alerts")]
        public async Task<IActionResult> WebSocketEndpoint()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await _webSocketManager.AddSocket(webSocket);

                var buffer = new byte[1024 * 4];
                WebSocketReceiveResult result;

                try
                {
                    do
                    {
                        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    }
                    while (!result.CloseStatus.HasValue);
                }
                finally
                {
                    await _webSocketManager.RemoveSocket(webSocket);
                }

                return new EmptyResult();
            }
            else
            {
                return BadRequest("WebSocket connection expected.");
            }
        }

        [HttpPost("receive")]
        public async Task<IActionResult> ReceiveAlert([FromBody] AlertModel alertModel)
        {
            if (alertModel == null || alertModel.Alerts == null || alertModel.Alerts.Count == 0)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid alert format",
                    Data = null
                });
            }

            try
            {
                foreach (var alert in alertModel.Alerts)
                {
                    Guid applicationId = new Guid("e6f01516-dcdf-4970-9133-69ab5477f082");
                    Guid ruleId = new Guid("5f8280dd-3168-413f-a6e9-093de3d06dd0");

                    string dynamicDescription = alert.Annotations.TryGetValue("description", out string desc) ? desc : "No description";
                    if (alert.Labels != null)
                    {
                        dynamicDescription += " | Labels: " + string.Join(", ", alert.Labels.Select(kv => $"{kv.Key}={kv.Value}"));
                    }

                    var existingAlert = await _context.Alerts
                        .FirstOrDefaultAsync(a => a.RuleId == ruleId && a.ApplicationId == applicationId && a.Message == dynamicDescription);

                    if (existingAlert != null)
                    {
                        if (existingAlert.Status != alert.Status || existingAlert.Message != dynamicDescription)
                        {
                            existingAlert.Status = alert.Status;
                            existingAlert.Message = dynamicDescription;
                            existingAlert.TriggeredAt = DateTime.UtcNow;

                            _context.Alerts.Update(existingAlert);
                        }
                    }
                    else
                    {
                        var newAlert = new Alert
                        {
                            Id = Guid.NewGuid(),
                            ApplicationId = applicationId,
                            RuleId = ruleId,
                            Message = dynamicDescription,
                            Status = alert.Status,
                            TriggeredAt = DateTime.UtcNow
                        };

                        _context.Alerts.Add(newAlert);
                    }
                }

                await _context.SaveChangesAsync();

                var alerts = await _alertService.GetAllAlertsAsync();
                await _webSocketManager.BroadcastMessageAsync("get_alerts", alerts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Error processing alert: {ex.Message}",
                    Data = null
                });
            }

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Alert processed successfully",
                Data = null
            });
        }

        [HttpGet("get_alerts")]
        public async Task<IActionResult> GetAlerts()
        {
            try
            {
                var alerts = await _alertService.GetAllAlertsAsync();
                return Ok(new ApiResponse<List<Alert>>
                {
                    Success = true,
                    Message = "Alerts retrieved successfully",
                    Data = alerts
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Internal Server Error: {ex.Message}",
                    Data = null
                });
            }
        }

        [HttpPost("resolve_alert/{alertId}")]
        public async Task<IActionResult> ResolveAlert(Guid alertId)
        {
            try
            {
                var result = await _alertService.ResolveAlertAsync(alertId);
                if (!result)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Alert not found or already resolved",
                        Data = null
                    });
                }

                var alerts = await _alertService.GetAllAlertsAsync();
                await _webSocketManager.BroadcastMessageAsync("get_alerts", alerts);

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Alert resolved successfully",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Internal Server Error: {ex.Message}",
                    Data = null
                });
            }
        }

        [HttpDelete("delete_alert/{alertId}")]
        public async Task<IActionResult> DeleteAlert(Guid alertId)
        {
            try
            {
                var result = await _alertService.DeleteAlertAsync(alertId);
                if (!result)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Alert not found",
                        Data = null
                    });
                }

                var alerts = await _alertService.GetAllAlertsAsync();
                await _webSocketManager.BroadcastMessageAsync("get_alerts", alerts);

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Alert deleted successfully",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Internal Server Error: {ex.Message}",
                    Data = null
                });
            }
        }
    }
}

// Models
public class AlertModel
{
    [JsonPropertyName("alerts")]
    public List<AlertDetail> Alerts { get; set; }
}

public class AlertDetail
{
    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string> Labels { get; set; }

    [JsonPropertyName("annotations")]
    public Dictionary<string, string> Annotations { get; set; }

    [JsonPropertyName("startsAt")]
    public DateTime StartsAt { get; set; }

    [JsonPropertyName("endsAt")]
    public DateTime EndsAt { get; set; }

    [JsonPropertyName("generatorURL")]
    public string GeneratorURL { get; set; }
}
