using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.DTOs.Request
{
    public class CreateRoomRequest
    {
        public int HostelId { get; set; }
        public int RoomTypeId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public int FloorNumber { get; set; } = 0;
        public int TotalBeds { get; set; }
        public decimal MonthlyRent { get; set; }
    }
}
