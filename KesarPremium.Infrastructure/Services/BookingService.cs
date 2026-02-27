using KesarPremium.Core.DTOs.Request;
using KesarPremium.Core.DTOs.Response;
using KesarPremium.Core.Entities;
using KesarPremium.Infrastructure.Data;
using KesarPremium.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Infrastructure.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IRoomRepository _roomRepo;
        private readonly INotificationService _notifService;
        private readonly ApplicationDbContext _db;

        public BookingService(IBookingRepository bookingRepo, IRoomRepository roomRepo,
            INotificationService notifService, ApplicationDbContext db)
        {
            _bookingRepo = bookingRepo;
            _roomRepo = roomRepo;
            _notifService = notifService;
            _db = db;
        }

        public async Task<ApiResponse<CreateBookingResponse>> CreateAsync(int userId, CreateBookingRequest req)
        {
            var room = await _db.Rooms.FindAsync(req.RoomId);
            if (room == null || !room.IsActive) return ApiResponse<CreateBookingResponse>.Fail("Room not found.");
            if (room.AvailableBeds <= 0) return ApiResponse<CreateBookingResponse>.Fail("No beds available in selected room.");

            var total = room.MonthlyRent * req.DurationMonths;
            var discount = req.DurationMonths >= 5 ? total * 0.30m : 0m;
            var final = total - discount;
            var bookingNum = $"KP-{DateTime.UtcNow:yyyy}{new Random().Next(10000, 99999)}";

            var booking = new Booking
            {
                UserId = userId,
                RoomId = req.RoomId,
                PlanId = req.PlanId,
                BookingNumber = bookingNum,
                CheckInDate = req.CheckInDate,
                CheckOutDate = req.CheckInDate.AddMonths(req.DurationMonths),
                TotalAmount = total,
                DiscountAmount = discount,
                FinalAmount = final,
                BookingStatus = "Pending",
                PaymentStatus = "Unpaid"
            };

            await _bookingRepo.AddAsync(booking);
            await _roomRepo.DecreaseBedAsync(req.RoomId);

            await _notifService.SendAsync(userId, "Booking Created",
                $"Your booking {bookingNum} has been created. Complete payment to confirm.", "BookingPending");

            return ApiResponse<CreateBookingResponse>.Ok(new CreateBookingResponse
            {
                BookingId = booking.BookingId,
                BookingNumber = bookingNum,
                FinalAmount = final,
                DiscountAmount = discount
            }, "Booking created. Please complete payment.");
        }

        public async Task<ApiResponse<List<BookingDto>>> GetUserBookingsAsync(int userId)
        {
            var bookings = await _bookingRepo.GetByUserAsync(userId);
            return ApiResponse<List<BookingDto>>.Ok(bookings);
        }

        public async Task<ApiResponse<List<BookingDto>>> GetAllAdminAsync(string? status, int page, int pageSize)
        {
            var bookings = await _bookingRepo.GetAllAdminAsync(status, page, pageSize);
            return ApiResponse<List<BookingDto>>.Ok(bookings);
        }

        public async Task<ApiResponse<BookingDto>> GetDetailAsync(int bookingId)
        {
            var booking = await _bookingRepo.GetDetailAsync(bookingId);
            return booking == null
                ? ApiResponse<BookingDto>.Fail("Booking not found.")
                : ApiResponse<BookingDto>.Ok(booking);
        }

        public async Task<ApiResponse<bool>> UpdateStatusAsync(UpdateBookingStatusRequest req)
        {
            var booking = await _bookingRepo.GetByIdAsync(req.BookingId);
            if (booking == null) return ApiResponse<bool>.Fail("Booking not found.");

            booking.BookingStatus = req.Status;
            booking.AdminNotes = req.AdminNotes;
            booking.UpdatedAt = DateTime.UtcNow;
            await _bookingRepo.UpdateAsync(booking);

            if (req.Status is "Rejected" or "Cancelled")
                await _roomRepo.IncreaseBedAsync(booking.RoomId);

            await _notifService.SendAsync(booking.UserId, $"Booking {req.Status}",
                $"Your booking {booking.BookingNumber} has been {req.Status.ToLower()}." +
                (!string.IsNullOrEmpty(req.AdminNotes) ? $" Note: {req.AdminNotes}" : ""), "BookingUpdate");

            return ApiResponse<bool>.Ok(true, $"Booking {req.Status.ToLower()} successfully.");
        }

        public async Task<ApiResponse<bool>> CancelAsync(int bookingId, int userId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null || booking.UserId != userId) return ApiResponse<bool>.Fail("Booking not found.");
            if (booking.BookingStatus == "CheckedOut") return ApiResponse<bool>.Fail("Cannot cancel a completed booking.");

            booking.BookingStatus = "Cancelled";
            booking.UpdatedAt = DateTime.UtcNow;
            await _bookingRepo.UpdateAsync(booking);
            await _roomRepo.IncreaseBedAsync(booking.RoomId);

            return ApiResponse<bool>.Ok(true, "Booking cancelled successfully.");
        }
    }

}
