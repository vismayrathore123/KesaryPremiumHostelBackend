using KesarPremium.Core.DTOs.Request;
using KesarPremium.Core.Entities;
using KesarPremium.Core.Interfaces;
using KesarPremium.Infrastructure.Data;
using KesarPremium.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace KesarPremium.Tests.Services;

public class BookingServiceTests
{
    private readonly Mock<IBookingRepository> _bookingRepoMock;
    private readonly Mock<IRoomRepository> _roomRepoMock;
    private readonly Mock<INotificationService> _notifMock;
    private readonly ApplicationDbContext _db;
    private readonly BookingService _bookingService;

    public BookingServiceTests()
    {
        _bookingRepoMock = new Mock<IBookingRepository>();
        _roomRepoMock = new Mock<IRoomRepository>();
        _notifMock = new Mock<INotificationService>();

        // In-memory DB for integration-style tests
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(options);

        _bookingService = new BookingService(
            _bookingRepoMock.Object,
            _roomRepoMock.Object,
            _notifMock.Object,
            _db
        );
    }

    // ── CREATE BOOKING ────────────────────────────────
    [Fact]
    public async Task CreateBooking_WithAvailableRoom_ShouldReturnSuccess()
    {
        // Arrange
        var room = new Room
        {
            RoomId = 1,
            HostelId = 1,
            RoomNumber = "101",
            MonthlyRent = 5000m,
            AvailableBeds = 2,
            IsActive = true,
            TotalBeds = 4,
            RoomTypeId = 1
        };
        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();

        _bookingRepoMock.Setup(r => r.AddAsync(It.IsAny<Booking>())).ReturnsAsync((Booking b) => b);
        _roomRepoMock.Setup(r => r.DecreaseBedAsync(1)).ReturnsAsync(true);
        _notifMock.Setup(n => n.SendAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        var request = new CreateBookingRequest
        {
            RoomId = 1,
            CheckInDate = DateTime.UtcNow.Date.AddDays(1),
            DurationMonths = 1
        };

        // Act
        var result = await _bookingService.CreateAsync(userId: 1, request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(5000m, result.Data.FinalAmount);
        Assert.Equal(0m, result.Data.DiscountAmount);
    }

    [Fact]
    public async Task CreateBooking_With5Months_ShouldApply30PercentDiscount()
    {
        // Arrange
        var room = new Room
        {
            RoomId = 2,
            HostelId = 1,
            RoomNumber = "102",
            MonthlyRent = 5000m,
            AvailableBeds = 3,
            IsActive = true,
            TotalBeds = 3,
            RoomTypeId = 1
        };
        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();

        _bookingRepoMock.Setup(r => r.AddAsync(It.IsAny<Booking>())).ReturnsAsync((Booking b) => b);
        _roomRepoMock.Setup(r => r.DecreaseBedAsync(2)).ReturnsAsync(true);
        _notifMock.Setup(n => n.SendAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        var request = new CreateBookingRequest
        {
            RoomId = 2,
            CheckInDate = DateTime.UtcNow.Date.AddDays(1),
            DurationMonths = 5
        };

        // Act
        var result = await _bookingService.CreateAsync(userId: 1, request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(7500m, result.Data!.DiscountAmount);   // 25000 * 30% = 7500
        Assert.Equal(17500m, result.Data.FinalAmount);      // 25000 - 7500 = 17500
    }

    [Fact]
    public async Task CreateBooking_WithNoAvailableBeds_ShouldReturnFailure()
    {
        // Arrange
        var room = new Room
        {
            RoomId = 3,
            HostelId = 1,
            RoomNumber = "103",
            MonthlyRent = 5000m,
            AvailableBeds = 0,    // No beds available
            IsActive = true,
            TotalBeds = 2,
            RoomTypeId = 1
        };
        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();

        var request = new CreateBookingRequest
        {
            RoomId = 3,
            CheckInDate = DateTime.UtcNow.Date.AddDays(1),
            DurationMonths = 1
        };

        // Act
        var result = await _bookingService.CreateAsync(userId: 1, request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("No beds available", result.Message);
    }

    [Fact]
    public async Task CreateBooking_WithInactiveRoom_ShouldReturnFailure()
    {
        // Arrange
        var room = new Room
        {
            RoomId = 4,
            HostelId = 1,
            RoomNumber = "104",
            MonthlyRent = 5000m,
            AvailableBeds = 3,
            IsActive = false,   // Inactive
            TotalBeds = 3,
            RoomTypeId = 1
        };
        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();

        var request = new CreateBookingRequest { RoomId = 4, CheckInDate = DateTime.UtcNow.AddDays(1), DurationMonths = 1 };

        // Act
        var result = await _bookingService.CreateAsync(userId: 1, request);

        // Assert
        Assert.False(result.Success);
    }

    // ── CANCEL BOOKING ────────────────────────────────
    [Fact]
    public async Task CancelBooking_ByOwner_ShouldReturnSuccess()
    {
        // Arrange
        var booking = new Booking
        {
            BookingId = 1,
            UserId = 1,
            RoomId = 1,
            BookingStatus = "Pending",
            BookingNumber = "KP-20240001"
        };
        _bookingRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);
        _bookingRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Booking>())).Returns(Task.CompletedTask);
        _roomRepoMock.Setup(r => r.IncreaseBedAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _bookingService.CancelAsync(bookingId: 1, userId: 1);

        // Assert
        Assert.True(result.Success);
        _roomRepoMock.Verify(r => r.IncreaseBedAsync(1), Times.Once);
    }

    [Fact]
    public async Task CancelBooking_ByDifferentUser_ShouldReturnFailure()
    {
        // Arrange
        var booking = new Booking { BookingId = 2, UserId = 99, RoomId = 1, BookingStatus = "Pending" };
        _bookingRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(booking);

        // Act
        var result = await _bookingService.CancelAsync(bookingId: 2, userId: 1);  // different userId

        // Assert
        Assert.False(result.Success);
    }

    [Fact]
    public async Task CancelBooking_AlreadyCheckedOut_ShouldReturnFailure()
    {
        // Arrange
        var booking = new Booking { BookingId = 3, UserId = 1, RoomId = 1, BookingStatus = "CheckedOut" };
        _bookingRepoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(booking);

        // Act
        var result = await _bookingService.CancelAsync(bookingId: 3, userId: 1);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("completed", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ── UPDATE STATUS ─────────────────────────────────
    [Fact]
    public async Task UpdateStatus_Confirm_ShouldNotRestoreBeds()
    {
        // Arrange
        var booking = new Booking { BookingId = 4, UserId = 1, RoomId = 1, BookingStatus = "Pending" };
        _bookingRepoMock.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(booking);
        _bookingRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Booking>())).Returns(Task.CompletedTask);
        _notifMock.Setup(n => n.SendAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>())).Returns(Task.CompletedTask);

        var request = new UpdateBookingStatusRequest { BookingId = 4, Status = "Confirmed" };

        // Act
        var result = await _bookingService.UpdateStatusAsync(request);

        // Assert
        Assert.True(result.Success);
        _roomRepoMock.Verify(r => r.IncreaseBedAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UpdateStatus_Reject_ShouldRestoreBeds()
    {
        // Arrange
        var booking = new Booking { BookingId = 5, UserId = 1, RoomId = 1, BookingStatus = "Pending" };
        _bookingRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(booking);
        _bookingRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Booking>())).Returns(Task.CompletedTask);
        _roomRepoMock.Setup(r => r.IncreaseBedAsync(1)).ReturnsAsync(true);
        _notifMock.Setup(n => n.SendAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>())).Returns(Task.CompletedTask);

        var request = new UpdateBookingStatusRequest { BookingId = 5, Status = "Rejected", AdminNotes = "Room unavailable." };

        // Act
        var result = await _bookingService.UpdateStatusAsync(request);

        // Assert
        Assert.True(result.Success);
        _roomRepoMock.Verify(r => r.IncreaseBedAsync(1), Times.Once);
    }

    public void Dispose() => _db.Dispose();
}
