using Alert_Management.Data;
using Alert_Management.DTOs;
using Alert_Management.Infterfaces;
using Alert_Management.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Alert_Management.Services
{
    public class AlertService : IAlertService
    {
        private readonly AlertDbContext _context;

        public AlertService(AlertDbContext context)
        {
            _context = context;
        }

        public async Task<List<Alert>> GetAllAlertsAsync()
        {
            return await _context.Alerts.OrderByDescending(x => x.TriggeredAt).ToListAsync();
        }

        // ✅ Resolve an alert (update status to 'Resolved')
        public async Task<bool> ResolveAlertAsync(Guid alertId)
        {
            var alert = await _context.Alerts.FindAsync(alertId);
            if (alert == null)
                return false;

            alert.Status = "resolved";
            alert.TriggeredAt = DateTime.UtcNow;

            _context.Alerts.Update(alert);
            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ Delete an alert
        public async Task<bool> DeleteAlertAsync(Guid alertId)
        {
            var alert = await _context.Alerts.FindAsync(alertId);
            if (alert == null)
                return false;

            _context.Alerts.Remove(alert);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
