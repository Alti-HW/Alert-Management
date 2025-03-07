namespace Alert_Management.DTOs
{
    public class UpdateAlertRuleDto
    {
        public string Expr { get; set; }  // Optional update to PromQL expression

        public string Duration { get; set; }  // Optional update to duration

        public string Severity { get; set; }  // Optional update to severity

        public string Description { get; set; }  // Optional update to description

        public Dictionary<string, string> Labels { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, string> Annotations { get; set; } = new Dictionary<string, string>();
    }
}
