using KesarPremium.Core.DTOs.Response;
using KesarPremium.Core.Entities;
using KesarPremium.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repo;

        public NotificationService(INotificationRepository repo) => _repo = repo;

        public async Task<ApiResponse<List<NotificationDto>>> GetUnreadAsync(int userId)
        {
            var list = await _repo.GetUnreadAsync(userId);
            return ApiResponse<List<NotificationDto>>.Ok(list);
        }

        public async Task<ApiResponse<bool>> MarkAllReadAsync(int userId)
        {
            await _repo.MarkAllReadAsync(userId);
            return ApiResponse<bool>.Ok(true);
        }

        public async Task SendAsync(int userId, string title, string message, string? type = null)
        {
            await _repo.AddAsync(new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                NotificationType = type
            });
        }
    }

}
