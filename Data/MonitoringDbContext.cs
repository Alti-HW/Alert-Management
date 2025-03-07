using System.Drawing;
using Microsoft.EntityFrameworkCore;
using Alert_Management.Models;

namespace Alert_Management.Data
{
    public class MonitoringDbContext : DbContext
    {
        public MonitoringDbContext(DbContextOptions<MonitoringDbContext> options) : base(options)
        {
        }

        // Tables related to energy monitoring
        public DbSet<EnergyConsumption> EnergyConsumptions { get; set; }
        public DbSet<Floor> Floors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EnergyConsumption>().ToTable("energy_consumption");
            modelBuilder.Entity<Floor>().ToTable("floors");
        }
    }
}
