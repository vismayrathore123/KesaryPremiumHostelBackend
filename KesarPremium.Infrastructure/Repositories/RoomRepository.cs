using KesarPremium.Core.DTOs.Response;
using KesarPremium.Core.Entities;
using KesarPremium.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KesarPremium.Infrastructure.Repositories.RoomRepository;

namespace KesarPremium.Infrastructure.Repositories
{
  
        public class RoomRepository : Repository<Room>, IRoomRepository
        {
            public RoomRepository(ApplicationDbContext db) : base(db) { }

            public async Task<List<RoomDto>> GetAvailableRoomsAsync(int hostelId)
            {
                return await _db.Rooms
                    .Include(r => r.RoomType).Include(r => r.Images)
                    .Where(r => r.HostelId == hostelId && r.IsActive && r.AvailableBeds > 0)
                    .Select(r => new RoomDto
                    {
                        RoomId = r.RoomId,
                        RoomNumber = r.RoomNumber,
                        FloorNumber = r.FloorNumber,
                        TotalBeds = r.TotalBeds,
                        AvailableBeds = r.AvailableBeds,
                        MonthlyRent = r.MonthlyRent,
                        RoomTypeName = r.RoomType.TypeName,
                        Images = r.Images.Select(i => new HostelImageDto { ImageId = i.ImageId, ImageUrl = i.ImageUrl }).ToList()
                    }).ToListAsync();
            }

            public async Task<bool> DecreaseBedAsync(int roomId)
            {
                var room = await _db.Rooms.FindAsync(roomId);
                if (room == null || room.AvailableBeds <= 0) return false;
                room.AvailableBeds--;
                room.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                return true;s
            }

            public async Task<bool> IncreaseBedAsync(int roomId)
            {
                var room = await _db.Rooms.FindAsync(roomId);
                if (room == null) return false;
                room.AvailableBeds++;
                room.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                return true;
            }
        }

 
}
