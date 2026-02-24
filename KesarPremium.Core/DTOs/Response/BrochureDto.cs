using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.DTOs.Response
{
    public class BrochureDto
    {
        public int BrochureId { get; set; }
        public string BrochureName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public int Version { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
