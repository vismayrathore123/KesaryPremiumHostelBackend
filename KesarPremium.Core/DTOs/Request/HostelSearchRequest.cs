using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.DTOs.Request
{
    public class HostelSearchRequest
    {
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public int? LocationId { get; set; }
        public decimal? MinRent { get; set; }
        public decimal? MaxRent { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
