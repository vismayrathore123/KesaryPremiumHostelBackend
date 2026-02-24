using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.DTOs.Request
{
    public class CreateBookingRequest
    {
        public int RoomId { get; set; }
        public int? PlanId { get; set; }
        public DateTime CheckInDate { get; set; }
        public int DurationMonths { get; set; } = 1;
    }
}
