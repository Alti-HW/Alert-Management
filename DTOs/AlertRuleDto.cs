namespace Alert_Management.DTOs
{
    public class AlertRuleDto
    {
        public string Name { get; set; }            // Unique name for the alert rule
        public string Metric { get; set; }          // The metric to monitor (e.g., energy consumption)
        public string Expression { get; set; }      // The PromQL expression for the rule
        public decimal Threshold { get; set; }      // Threshold value to trigger the alert
        public string Severity { get; set; }        // Alert severity level (e.g., critical, warning)
        public string Duration { get; set; }        // How long the condition should be true before firing
        public string Description { get; set; }     // Description of the alert rule
        public bool IsEnabled { get; set; }         // Flag to enable or disable the rule
    }

}
