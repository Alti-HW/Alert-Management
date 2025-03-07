using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Alert_Management.DTOs
{
    public class CreateAlertRuleDto
    {
        [Required]
        public string Alert { get; set; }  // Unique alert name

        [Required]
        public string Expr { get; set; }  // PromQL expression

        public string Duration { get; set; } = "1m";  // Default 1 minute

        [Required]
        public string Severity { get; set; }  // Critical, Warning, etc.

        public string Description { get; set; }  // Alert message

        public Dictionary<string, string> Labels { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, string> Annotations { get; set; } = new Dictionary<string, string>();
    }
}
