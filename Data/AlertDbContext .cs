using System.Collections.Generic;
using System.Drawing;
using System.Reflection.Emit;
using Alert_Management.Models;
using Microsoft.EntityFrameworkCore;

namespace Alert_Management.Data
{
    public class AlertDbContext : DbContext
    {
        public AlertDbContext(DbContextOptions<AlertDbContext> options) : base(options)
        {
        }

        // DbSet properties for each entity
        public DbSet<Application> Applications { get; set; }
        public DbSet<AlertRule> AlertRules { get; set; }
        public DbSet<Alert> Alerts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Alert>().ToTable("alerts");
            modelBuilder.Entity<AlertRule>().ToTable("alert_rules");

        }
    }
}
