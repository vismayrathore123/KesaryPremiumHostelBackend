using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.DTOs.Response
{
    public class BookingDto
    {
        public int BookingId { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public string BookingStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public DateTime BookingDate { get; set; }
        public string? AdminNotes { get; set; }

        // User info
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string? UserPhone { get; set; }

        // Room / Hostel info
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public int HostelId { get; set; }
        public string HostelName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;

        // Payment info
        public string? TransactionId { get; set; }
        public string? PaymentGateway { get; set; }
        public decimal? PaidAmount { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
