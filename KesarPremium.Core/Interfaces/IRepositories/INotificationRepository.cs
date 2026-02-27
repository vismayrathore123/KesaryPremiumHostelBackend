using KesarPremium.Core.DTOs.Response;
using KesarPremium.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.Interfaces.IRepositories
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Task<List<NotificationDto>> GetUnreadAsync(int userId);
        Task MarkAllReadAsync(int userId);
    }
}
