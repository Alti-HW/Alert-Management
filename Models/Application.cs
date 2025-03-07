using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Alert_Management.Models;

namespace Alert_Management.Models
{
    public class Application
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid(); // Primary Key

        [Required]
        [MaxLength(50)]
        public string ApplicationId { get; set; } // External Identifier

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public ICollection<AlertRule> AlertRules { get; set; } = new List<AlertRule>();
    }
}
