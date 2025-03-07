using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alert_Management.Models
{
    [Table("alerts", Schema = "public")] // Ensuring schema consistency
    public class Alert
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid(); // Ensuring default value is set

        [Required]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        [Required]
        [Column("rule_id")]
        public Guid RuleId { get; set; }

        [Required]
        [Column("message")]
        public string Message { get; set; }

        [Required]
        [Column("status")]
        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Default status

        [Column("triggered_at")]
        public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
    }
}
