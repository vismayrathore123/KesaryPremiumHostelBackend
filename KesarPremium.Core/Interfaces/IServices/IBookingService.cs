using KesarPremium.Core.DTOs.Request;
using KesarPremium.Core.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.Interfaces.IServices
{
    public interface IBookingService
    {
        Task<ApiResponse<CreateBookingResponse>> CreateAsync(int userId, CreateBookingRequest request);
        Task<ApiResponse<List<BookingDto>>> GetUserBookingsAsync(int userId);
        Task<ApiResponse<List<BookingDto>>> GetAllAdminAsync(string? status, int page, int pageSize);
        Task<ApiResponse<BookingDto>> GetDetailAsync(int bookingId);
        Task<ApiResponse<bool>> UpdateStatusAsync(UpdateBookingStatusRequest request);
        Task<ApiResponse<bool>> CancelAsync(int bookingId, int userId);
    }
}
