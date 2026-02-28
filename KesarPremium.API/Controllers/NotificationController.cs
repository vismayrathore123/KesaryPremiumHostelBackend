using KesarPremium.Core.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KesarPremium.API.Controllers
{
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController : BaseController
    {
        private readonly INotificationService _notifService;
        public NotificationController(INotificationService notifService) => _notifService = notifService;

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnread()
        {
            var result = await _notifService.GetUnreadAsync(CurrentUserId);
            return Ok(result);
        }

        [HttpPost("mark-read")]
        public async Task<IActionResult> MarkAllRead()
        {
            var result = await _notifService.MarkAllReadAsync(CurrentUserId);
            return Ok(result);
        }
    }
}
