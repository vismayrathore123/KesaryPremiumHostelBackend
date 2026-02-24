using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.DTOs.Request
{
    public class CreatePricingPlanRequest
    {
        public int HostelId { get; set; }
        public int RoomTypeId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public int DurationMonths { get; set; }
        public decimal BaseRent { get; set; }
        public decimal DiscountPercent { get; set; } = 0;
    }
}
