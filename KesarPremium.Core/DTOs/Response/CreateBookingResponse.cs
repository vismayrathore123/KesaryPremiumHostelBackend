using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.DTOs.Response
{
    public class CreateBookingResponse
    {
        public int BookingId { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public decimal FinalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
    }
}
