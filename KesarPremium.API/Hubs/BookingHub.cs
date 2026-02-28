using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace KesarPremium.API.Hubs;

/// <summary>
/// SignalR Hub for real-time booking and availability updates.
/// Connect from frontend: const connection = new signalR.HubConnectionBuilder().withUrl("/hubs/booking").build();
/// </summary>
[Authorize]
public class BookingHub : Hub
{
    private readonly ILogger<BookingHub> _logger;

    public BookingHub(ILogger<BookingHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects to the hub.
    /// Admin joins "admins" group automatically.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        var role = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        _logger.LogInformation("Client connected: {UserId}, Role: {Role}", userId, role);

        if (role == "Admin")
            await Groups.AddToGroupAsync(Context.ConnectionId, "admins");

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {UserId}", Context.UserIdentifier);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Client calls this to subscribe to updates for a specific hostel.
    /// e.g. connection.invoke("JoinHostelGroup", "5");
    /// </summary>
    public async Task JoinHostelGroup(string hostelId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"hostel-{hostelId}");
        _logger.LogInformation("User {UserId} joined hostel group {HostelId}", Context.UserIdentifier, hostelId);
    }

    /// <summary>
    /// Client calls this to unsubscribe from a hostel group.
    /// </summary>
    public async Task LeaveHostelGroup(string hostelId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"hostel-{hostelId}");
    }
}

/// <summary>
/// Service to push real-time events from backend services.
/// Inject IHubContext&lt;BookingHub&gt; wherever you need to push events.
/// </summary>
public class BookingHubService
{
    private readonly IHubContext<BookingHub> _hub;

    public BookingHubService(IHubContext<BookingHub> hub)
    {
        _hub = hub;
    }

    /// <summary>
    /// Broadcast room availability update to all clients watching a hostel.
    /// Call this after a booking is created or cancelled.
    /// </summary>
    public async Task SendRoomAvailabilityUpdate(int hostelId, int roomId, int availableBeds)
    {
        await _hub.Clients.Group($"hostel-{hostelId}").SendAsync("RoomAvailabilityUpdate", new
        {
            hostelId,
            roomId,
            availableBeds,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Notify a specific user about their booking status change.
    /// </summary>
    public async Task SendBookingStatusUpdate(string userId, int bookingId, string status, string bookingNumber)
    {
        await _hub.Clients.User(userId).SendAsync("BookingStatusUpdate", new
        {
            bookingId,
            status,
            bookingNumber,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Notify all admins about a new enquiry or booking.
    /// </summary>
    public async Task SendAdminAlert(string title, string message, object? data = null)
    {
        await _hub.Clients.Group("admins").SendAsync("AdminAlert", new
        {
            title,
            message,
            data,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Notify a user of a new notification.
    /// </summary>
    public async Task SendNotification(string userId, string title, string message, string type)
    {
        await _hub.Clients.User(userId).SendAsync("NewNotification", new
        {
            title,
            message,
            type,
            timestamp = DateTime.UtcNow
        });
    }
}
