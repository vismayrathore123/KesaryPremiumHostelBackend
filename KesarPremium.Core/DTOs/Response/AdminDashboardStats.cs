using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.DTOs.Response
{
    public class AdminDashboardStats
    {
        public int TotalUsers { get; set; }
        public int TotalHostels { get; set; }
        public int ActiveBookings { get; set; }
        public int PendingBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public int NewEnquiries { get; set; }
        public int TotalAvailableBeds { get; set; }
    }
}
