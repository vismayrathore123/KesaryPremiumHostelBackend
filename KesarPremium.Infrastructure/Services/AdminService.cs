using KesarPremium.Core.DTOs.Response;
using KesarPremium.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Infrastructure.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _db;

        public AdminService(ApplicationDbContext db) => _db = db;

        public async Task<ApiResponse<AdminDashboardStats>> GetDashboardStatsAsync()
        {
            var stats = new AdminDashboardStats
            {
                TotalUsers = await _db.Users.CountAsync(u => u.RoleId == 2),
                TotalHostels = await _db.Hostels.CountAsync(h => h.IsActive),
                ActiveBookings = await _db.Bookings.CountAsync(b => b.BookingStatus == "Confirmed"),
                PendingBookings = await _db.Bookings.CountAsync(b => b.BookingStatus == "Pending"),
                TotalRevenue = await _db.Payments.Where(p => p.PaymentStatus == "Success").SumAsync(p => p.Amount),
                NewEnquiries = await _db.Enquiries.CountAsync(e => e.EnquiryStatus == "New"),
                TotalAvailableBeds = await _db.Rooms.Where(r => r.IsActive).SumAsync(r => r.AvailableBeds)
            };

            return ApiResponse<AdminDashboardStats>.Ok(stats);
        }
    }

}
