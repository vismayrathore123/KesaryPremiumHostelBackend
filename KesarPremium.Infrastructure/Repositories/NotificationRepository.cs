using KesarPremium.Core.DTOs.Response;
using KesarPremium.Core.Entities;
using KesarPremium.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Infrastructure.Repositories
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        public NotificationRepository(ApplicationDbContext db) : base(db) { }

        public async Task<List<NotificationDto>> GetUnreadAsync(int userId)
            => await _db.Notifications.Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    Title = n.Title,
                    Message = n.Message,
                    NotificationType = n.NotificationType,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                }).ToListAsync();

        public async Task MarkAllReadAsync(int userId)
        {
            var notifs = await _db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
            notifs.ForEach(n => { n.IsRead = true; n.ReadAt = DateTime.UtcNow; });
            await _db.SaveChangesAsync();
        }
    }
}
