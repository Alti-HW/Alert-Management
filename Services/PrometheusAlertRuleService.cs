using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Alert_Management.Data;
using Alert_Management.DTOs;
using Alert_Management.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Alert_Management.Services
{
    public class PrometheusAlertRuleService : IAlertRuleService
    {
        private readonly string _alertRulesFilePath = "./Prometheus_Config/Prometheus/alert_rules.yml";
        private readonly HttpClient _httpClient;
        private readonly ILogger<PrometheusAlertRuleService> _logger;
        private readonly AlertDbContext _context;
        public PrometheusAlertRuleService(IHttpClientFactory httpClientFactory, ILogger<PrometheusAlertRuleService> logger, AlertDbContext context)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
            _context = context;
        }

        public async Task<List<PrometheusAlertRule>> GetAllAlertRulesAsync()
        {
            if (!File.Exists(_alertRulesFilePath))
                return new List<PrometheusAlertRule>();

            var yamlContent = await File.ReadAllTextAsync(_alertRulesFilePath);
            var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            var rulesFile = deserializer.Deserialize<AlertRulesFile>(yamlContent);

            return rulesFile.Groups.FirstOrDefault()?.Rules ?? new List<PrometheusAlertRule>();
        }

        public async Task<PrometheusAlertRule> GetAlertRuleByIdAsync(string alertName)
        {
            var rules = await GetAllAlertRulesAsync();
            return rules.FirstOrDefault(r => r.Alert == alertName);
        }

        public async Task<PrometheusAlertRule> CreateAlertRuleAsync(CreateAlertRuleDto newRule)
        {
            var rules = await GetAllAlertRulesAsync();

            var alertRule = new PrometheusAlertRule
            {
                Alert = newRule.Alert,
                Expr = newRule.Expr,
                For = newRule.Duration ?? "1m",  // Default duration if not provided
                Labels = new Dictionary<string, string> { { "severity", newRule.Severity } },
                Annotations = new Dictionary<string, string> { { "description", newRule.Description } }
            };

            // Merge additional labels if provided
            if (newRule.Labels != null)
            {
                foreach (var label in newRule.Labels)
                {
                    alertRule.Labels[label.Key] = label.Value;
                }
            }

            // Merge additional annotations if provided
            if (newRule.Annotations != null)
            {
                foreach (var annotation in newRule.Annotations)
                {
                    alertRule.Annotations[annotation.Key] = annotation.Value;
                }
            }

            rules.Add(alertRule);
            await SaveRulesToFile(rules);
            return alertRule;
        }

        public async Task<PrometheusAlertRule> UpdateAlertRuleAsync(string alertName, UpdateAlertRuleDto updatedRule)
        {
            var rules = await GetAllAlertRulesAsync();
            var rule = rules.FirstOrDefault(r => r.Alert == alertName);
            if (rule == null) return null;

            rule.Expr = updatedRule.Expr ?? rule.Expr;
            rule.For = updatedRule.Duration ?? rule.For;
            rule.Labels["severity"] = updatedRule.Severity ?? rule.Labels["severity"];
            rule.Annotations["description"] = updatedRule.Description ?? rule.Annotations["description"];

            // Update additional labels if provided
            if (updatedRule.Labels != null)
            {
                foreach (var label in updatedRule.Labels)
                {
                    rule.Labels[label.Key] = label.Value;
                }
            }

            // Update additional annotations if provided
            if (updatedRule.Annotations != null)
            {
                foreach (var annotation in updatedRule.Annotations)
                {
                    rule.Annotations[annotation.Key] = annotation.Value;
                }
            }

            await SaveRulesToFile(rules);
            return rule;
        }

        public async Task<List<Alert>> GetAllAlertsAsync()
        {
            return await _context.Alerts.OrderByDescending(x => x.TriggeredAt).ToListAsync();
        }


        public async Task<bool> DeleteAlertRuleAsync(string alertName)
        {
            var rules = await GetAllAlertRulesAsync();
            var ruleToRemove = rules.FirstOrDefault(r => r.Alert == alertName);
            if (ruleToRemove == null) return false;

            rules.Remove(ruleToRemove);
            await SaveRulesToFile(rules);
            await ReloadPrometheusConfig();
            return true;
        }

        private async Task SaveRulesToFile(List<PrometheusAlertRule> rules)
        {
            var serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            var yamlContent = serializer.Serialize(new AlertRulesFile
            {
                Groups = new List<AlertRuleGroup>
                {
                    new AlertRuleGroup
                    {
                        Name = "EnergyAlerts",
                        Rules = rules
                    }
                }
            });

            await File.WriteAllTextAsync(_alertRulesFilePath, yamlContent);
            await ReloadPrometheusConfig();
        }

        private async Task<bool> ReloadPrometheusConfig()
        {
            try
            {
                var response = await _httpClient.PostAsync("http://localhost:9090/-/reload", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while reloading Prometheus configuration.");
                return false;
            }
        }
    }

    public class AlertRulesFile
    {
        public List<AlertRuleGroup> Groups { get; set; }
    }

    public class AlertRuleGroup
    {
        public string Name { get; set; }
        public List<PrometheusAlertRule> Rules { get; set; }
    }
}
