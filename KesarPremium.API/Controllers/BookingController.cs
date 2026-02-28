using KesarPremium.Core.DTOs.Request;
using KesarPremium.Core.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KesarPremium.API.Controllers
{
    [Route("api/bookings")]
    [Authorize]
    public class BookingController : BaseController
    {
        private readonly IBookingService _bookingService;
        public BookingController(IBookingService bookingService) => _bookingService = bookingService;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBookingRequest request)
        {
            var result = await _bookingService.CreateAsync(CurrentUserId, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyBookings()
        {
            var result = await _bookingService.GetUserBookingsAsync(CurrentUserId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var result = await _bookingService.GetDetailAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _bookingService.CancelAsync(id, CurrentUserId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // Admin only
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _bookingService.GetAllAdminAsync(status, page, pageSize);
            return Ok(result);
        }

        [HttpPut("status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateBookingStatusRequest request)
        {
            var result = await _bookingService.UpdateStatusAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
