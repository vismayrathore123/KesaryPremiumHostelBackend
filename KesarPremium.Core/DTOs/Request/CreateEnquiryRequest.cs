using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.DTOs.Request
{
    public class CreateEnquiryRequest
    {
        public int? HostelId { get; set; }
        public string? Subject { get; set; }
        public string? Message { get; set; }
    }
}
