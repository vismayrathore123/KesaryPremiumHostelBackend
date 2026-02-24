using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.DTOs.Request
{
    public class CreateHostelRequest
    {
        public string HostelName { get; set; } = string.Empty;
        public int LocationId { get; set; }
        public int CategoryId { get; set; }
        public string? Description { get; set; }
        public string? ContactNumber { get; set; }
        public string? WhatsAppNumber { get; set; }
        public List<int> FacilityIds { get; set; } = new();
    }
}
