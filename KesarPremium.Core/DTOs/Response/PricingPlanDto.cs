using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.DTOs.Response
{
    public class PricingPlanDto
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public int DurationMonths { get; set; }
        public decimal BaseRent { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal FinalRent { get; set; }
        public string RoomTypeName { get; set; } = string.Empty;
    }
}
