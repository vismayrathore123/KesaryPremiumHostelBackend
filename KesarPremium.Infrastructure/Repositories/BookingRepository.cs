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
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        public BookingRepository(ApplicationDbContext db) : base(db) { }

        public async Task<List<BookingDto>> GetByUserAsync(int userId)
        {
            return await _db.Bookings
                .Include(b => b.Room).ThenInclude(r => r.Hostel).ThenInclude(h => h.Location)
                .Include(b => b.Room).ThenInclude(r => r.Hostel).ThenInclude(h => h.Category)
                .Include(b => b.Payments)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => MapBookingDto(b))
                .ToListAsync();
        }

        public async Task<List<BookingDto>> GetAllAdminAsync(string? status, int page, int pageSize)
        {
            var query = _db.Bookings
                .Include(b => b.User)
                .Include(b => b.Room).ThenInclude(r => r.Hostel).ThenInclude(h => h.Location)
                .Include(b => b.Room).ThenInclude(r => r.Hostel).ThenInclude(h => h.Category)
                .Include(b => b.Payments)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(b => b.BookingStatus == status);

            return await query.OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(b => MapBookingDto(b))
                .ToListAsync();
        }

        public async Task<BookingDto?> GetDetailAsync(int bookingId)
        {
            var b = await _db.Bookings
                .Include(b => b.User)
                .Include(b => b.Room).ThenInclude(r => r.Hostel).ThenInclude(h => h.Location)
                .Include(b => b.Room).ThenInclude(r => r.Hostel).ThenInclude(h => h.Category)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            return b == null ? null : MapBookingDto(b);
        }

        private static BookingDto MapBookingDto(Booking b)
        {
            var payment = b.Payments?.FirstOrDefault(p => p.PaymentStatus == "Success");
            return new BookingDto
            {
                BookingId = b.BookingId,
                BookingNumber = b.BookingNumber,
                BookingStatus = b.BookingStatus,
                PaymentStatus = b.PaymentStatus,
                CheckInDate = b.CheckInDate,
                CheckOutDate = b.CheckOutDate,
                TotalAmount = b.TotalAmount,
                DiscountAmount = b.DiscountAmount,
                FinalAmount = b.FinalAmount,
                BookingDate = b.CreatedAt,
                AdminNotes = b.AdminNotes,
                UserId = b.UserId,
                UserName = b.User?.FullName ?? "",
                UserEmail = b.User?.Email ?? "",
                UserPhone = b.User?.PhoneNumber,
                RoomId = b.RoomId,
                RoomNumber = b.Room?.RoomNumber ?? "",
                HostelId = b.Room?.HostelId ?? 0,
                HostelName = b.Room?.Hostel?.HostelName ?? "",
                CategoryName = b.Room?.Hostel?.Category?.CategoryName ?? "",
                LocationName = b.Room?.Hostel?.Location?.LocationName ?? "",
                TransactionId = payment?.TransactionId,
                PaymentGateway = payment?.PaymentGateway,
                PaidAmount = payment?.Amount,
                PaidAt = payment?.PaidAt
            };
        }
    }

}
