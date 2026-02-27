using KesarPremium.Core.DTOs.Response;
using KesarPremium.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.Interfaces.IRepositories
{
    public interface IRoomRepository : IRepository<Room>
    {
        Task<List<RoomDto>> GetAvailableRoomsAsync(int hostelId);
        Task<bool> DecreaseBedAsync(int roomId);
        Task<bool> IncreaseBedAsync(int roomId);
    }

}
