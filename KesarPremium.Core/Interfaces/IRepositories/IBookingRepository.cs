using KesarPremium.Core.DTOs.Response;
using KesarPremium.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.Interfaces.IRepositories
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<List<BookingDto>> GetByUserAsync(int userId);
        Task<List<BookingDto>> GetAllAdminAsync(string? status, int page, int pageSize);
        Task<BookingDto?> GetDetailAsync(int bookingId);
    }
}
