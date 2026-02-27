using KesarPremium.Core.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.Interfaces.IServices
{
    public interface INotificationService
    {
        Task<ApiResponse<List<NotificationDto>>> GetUnreadAsync(int userId);
        Task<ApiResponse<bool>> MarkAllReadAsync(int userId);
        Task SendAsync(int userId, string title, string message, string? type = null);
    }

}
