using KesarPremium.Core.DTOs.Request;
using KesarPremium.Core.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.Interfaces.IServices
{
    public interface IRoomService
    {
        Task<ApiResponse<List<RoomDto>>> GetAvailableAsync(int hostelId);
        Task<ApiResponse<RoomDto>> CreateAsync(CreateRoomRequest request);
        Task<ApiResponse<RoomDto>> UpdateAsync(UpdateRoomRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int roomId);
    }
}
