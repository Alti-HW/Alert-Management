using Alert_Management.DTOs;
using Alert_Management.Models;
using Alert_Management.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/alerts")]
[ApiController]
public class AlertRulesController : ControllerBase
{
    private readonly IAlertRuleService _alertRuleService;

    public AlertRulesController(IAlertRuleService alertRuleService)
    {
        _alertRuleService = alertRuleService;
    }

    // Get all alert rules
    [HttpGet("rules")]
    public async Task<ActionResult<ApiResponse<List<PrometheusAlertRule>>>> GetAlertRules()
    {
        var alertRules = await _alertRuleService.GetAllAlertRulesAsync();
        return Ok(new ApiResponse<List<PrometheusAlertRule>>
        {
            Success = true,
            Message = "Alert rules retrieved successfully.",
            Data = alertRules
        });
    }

    // Get a specific alert rule by name
    [HttpGet("rules/{alertName}")]
    public async Task<ActionResult<ApiResponse<PrometheusAlertRule>>> GetAlertRuleById(string alertName)
    {
        var alertRule = await _alertRuleService.GetAlertRuleByIdAsync(alertName);
        if (alertRule == null)
            return NotFound(new ApiResponse<PrometheusAlertRule>
            {
                Success = false,
                Message = "Alert rule not found.",
                Data = null
            });

        return Ok(new ApiResponse<PrometheusAlertRule>
        {
            Success = true,
            Message = "Alert rule retrieved successfully.",
            Data = alertRule
        });
    }

    // Create a new alert rule
    [HttpPost("rules")]
    public async Task<ActionResult<ApiResponse<PrometheusAlertRule>>> CreateAlertRule([FromBody] CreateAlertRuleDto dto)
    {
        if (dto == null)
            return BadRequest(new ApiResponse<PrometheusAlertRule>
            {
                Success = false,
                Message = "Invalid alert rule data.",
                Data = null
            });

        var createdRule = await _alertRuleService.CreateAlertRuleAsync(dto);
        return CreatedAtAction(nameof(GetAlertRuleById), new { alertName = createdRule.Alert },
            new ApiResponse<PrometheusAlertRule>
            {
                Success = true,
                Message = "Alert rule created successfully.",
                Data = createdRule
            });
    }

    // Update an existing alert rule
    [HttpPut("rules/{alertName}")]
    public async Task<ActionResult<ApiResponse<PrometheusAlertRule>>> UpdateAlertRule(string alertName, [FromBody] UpdateAlertRuleDto dto)
    {
        var updatedRule = await _alertRuleService.UpdateAlertRuleAsync(alertName, dto);
        if (updatedRule == null)
            return NotFound(new ApiResponse<PrometheusAlertRule>
            {
                Success = false,
                Message = "Alert rule not found.",
                Data = null
            });

        return Ok(new ApiResponse<PrometheusAlertRule>
        {
            Success = true,
            Message = "Alert rule updated successfully.",
            Data = updatedRule
        });
    }

    // Delete an alert rule
    [HttpDelete("rules/{alertName}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAlertRule(string alertName)
    {
        var isDeleted = await _alertRuleService.DeleteAlertRuleAsync(alertName);
        if (!isDeleted)
            return NotFound(new ApiResponse<bool>
            {
                Success = false,
                Message = "Alert rule not found.",
                Data = false
            });

        return Ok(new ApiResponse<bool>
        {
            Success = true,
            Message = "Alert rule deleted successfully.",
            Data = true
        });
    }
}
