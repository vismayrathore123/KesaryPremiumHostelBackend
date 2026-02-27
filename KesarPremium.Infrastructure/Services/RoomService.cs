using KesarPremium.Core.DTOs.Request;
using KesarPremium.Core.DTOs.Response;
using KesarPremium.Core.Entities;
using KesarPremium.Infrastructure.Data;
using KesarPremium.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Infrastructure.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _repo;
        private readonly ApplicationDbContext _db;

        public RoomService(IRoomRepository repo, ApplicationDbContext db)
        {
            _repo = repo;
            _db = db;
        }

        public async Task<ApiResponse<List<RoomDto>>> GetAvailableAsync(int hostelId)
        {
            var rooms = await _repo.GetAvailableRoomsAsync(hostelId);
            return ApiResponse<List<RoomDto>>.Ok(rooms);
        }

        public async Task<ApiResponse<RoomDto>> CreateAsync(CreateRoomRequest req)
        {
            var room = new Room
            {
                HostelId = req.HostelId,
                RoomTypeId = req.RoomTypeId,
                RoomNumber = req.RoomNumber,
                FloorNumber = req.FloorNumber,
                TotalBeds = req.TotalBeds,
                AvailableBeds = req.TotalBeds,
                MonthlyRent = req.MonthlyRent
            };

            await _repo.AddAsync(room);

            // Update hostel totals
            var hostel = await _db.Hostels.FindAsync(req.HostelId);
            if (hostel != null) { hostel.TotalRooms++; hostel.TotalBeds += req.TotalBeds; await _db.SaveChangesAsync(); }

            return ApiResponse<RoomDto>.Ok(new RoomDto
            {
                RoomId = room.RoomId,
                RoomNumber = room.RoomNumber,
                TotalBeds = room.TotalBeds,
                AvailableBeds = room.AvailableBeds,
                MonthlyRent = room.MonthlyRent
            }, "Room created successfully.");
        }

        public async Task<ApiResponse<RoomDto>> UpdateAsync(UpdateRoomRequest req)
        {
            var room = await _repo.GetByIdAsync(req.RoomId);
            if (room == null) return ApiResponse<RoomDto>.Fail("Room not found.");

            room.RoomNumber = req.RoomNumber;
            room.FloorNumber = req.FloorNumber;
            room.RoomTypeId = req.RoomTypeId;
            room.MonthlyRent = req.MonthlyRent;
            room.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(room);
            return ApiResponse<RoomDto>.Ok(new RoomDto { RoomId = room.RoomId, RoomNumber = room.RoomNumber });
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int roomId)
        {
            var room = await _repo.GetByIdAsync(roomId);
            if (room == null) return ApiResponse<bool>.Fail("Room not found.");
            room.IsActive = false;
            room.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(room);
            return ApiResponse<bool>.Ok(true, "Room deactivated.");
        }
    }
}
