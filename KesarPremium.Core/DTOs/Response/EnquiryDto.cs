using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.DTOs.Response
{
    public class EnquiryDto
    {
        public int EnquiryId { get; set; }
        public string? Subject { get; set; }
        public string? Message { get; set; }
        public string EnquiryStatus { get; set; } = string.Empty;
        public string? AdminNotes { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string? UserPhone { get; set; }
        public string? HostelName { get; set; }
    }
}
