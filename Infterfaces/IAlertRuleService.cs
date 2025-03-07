using Alert_Management.DTOs;
using Alert_Management.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Alert_Management.Services
{
    public interface IAlertRuleService
    {
        Task<List<PrometheusAlertRule>> GetAllAlertRulesAsync();
        Task<PrometheusAlertRule> GetAlertRuleByIdAsync(string alertName);
        Task<PrometheusAlertRule> CreateAlertRuleAsync(CreateAlertRuleDto dto);
        Task<PrometheusAlertRule> UpdateAlertRuleAsync(string alertName, UpdateAlertRuleDto dto);
        Task<bool> DeleteAlertRuleAsync(string alertName);
       
    }
}
