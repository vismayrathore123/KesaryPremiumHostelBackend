using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.DTOs.Response
{
    public class HostelDetailDto:HostelListDto
    {
        public int TotalRooms { get; set; }
        public int TotalBeds { get; set; }
        public List<FacilityDto> Facilities { get; set; } = new();
        public List<HostelImageDto> Images { get; set; } = new();
        public List<RoomDto> Rooms { get; set; } = new();
        public List<SharedAreaDto> SharedAreas { get; set; } = new();
        public List<PricingPlanDto> PricingPlans { get; set; } = new();
        public BrochureDto? LatestBrochure { get; set; }
    }
}
