using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.DTOs.Request
{
    public class UpdateEnquiryRequest
    {
        public int EnquiryId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? AdminNotes { get; set; }
        public DateTime? FollowUpDate { get; set; }
    }
}
