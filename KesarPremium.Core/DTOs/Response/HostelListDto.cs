using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.DTOs.Response
{
    public class HostelListDto
    {
        public int HostelId { get; set; }
        public string HostelName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? PinCode { get; set; }
        public int TotalAvailableBeds { get; set; }
        public decimal? StartingRentFrom { get; set; }
        public string? PrimaryImage { get; set; }
        public string? ContactNumber { get; set; }
        public string? WhatsAppNumber { get; set; }
    }
}

