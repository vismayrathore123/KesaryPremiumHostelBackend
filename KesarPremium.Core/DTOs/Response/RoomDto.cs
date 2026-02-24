using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.DTOs.Response
{
    public class RoomDto
    {
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public int FloorNumber { get; set; }
        public int TotalBeds { get; set; }
        public int AvailableBeds { get; set; }
        public decimal MonthlyRent { get; set; }
        public string RoomTypeName { get; set; } = string.Empty;
        public string AvailabilityStatus => AvailableBeds > 0 ? "Available" : "Full";
        public decimal Rent5Months => MonthlyRent * 5 * 0.70m;
        public List<HostelImageDto> Images { get; set; } = new();
    }
}
