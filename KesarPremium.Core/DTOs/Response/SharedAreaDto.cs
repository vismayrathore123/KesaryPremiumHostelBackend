using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.DTOs.Response
{
    public class SharedAreaDto
    {
        public int SharedAreaId { get; set; }
        public string AreaName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<string> Images { get; set; } = new();
    }
}
