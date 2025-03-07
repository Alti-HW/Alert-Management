using System;
using System.Collections.Generic;

namespace Alert_Management.Models
{
    public class PrometheusAlertRule
    {
        public string Alert { get; set; }  // Matches 'alert' field in YAML
        public string Expr { get; set; }  // Matches 'expr' (expression for alert rule)
        public string For { get; set; }  // Matches 'for' (duration)
        public Dictionary<string, string> Labels { get; set; }  // Matches 'labels'
        public Dictionary<string, string> Annotations { get; set; }  // Matches 'annotations'
    }
}
