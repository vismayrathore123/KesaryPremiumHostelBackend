using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.DTOs.Request
{
    public class UpdateRoomRequest : CreateRoomRequest
    {
        public int RoomId { get; set; }

    }
}
