using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alert_Management.Models
{
    public class AlertRule
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        [Required]
        [Column("name")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Column("metric")]
        [StringLength(100)]
        public string Metric { get; set; } // E.g., energy_consumed_kwh, peak_load_kw

        [Required]
        [Column("expression")]
        [StringLength(2)]
        public string Expression { get; set; } // Allowed values: >, <, >=, <=, ==

        [Required]
        [Column("threshold")]
        public decimal Threshold { get; set; } // Changed from double to decimal

        [Required]
        [Column("severity")]
        [StringLength(20)]
        public string Severity { get; set; } // E.g., "Critical", "Warning"

        [Required]
        [Column("duration")]
        [StringLength(20)]
        public string Duration { get; set; } // E.g., "5m", "10m"

        [Required]
        [Column("description")]
        public string Description { get; set; }

        [Column("is_enabled")]
        public bool IsEnabled { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
