using System.ComponentModel.DataAnnotations;

namespace KesarPremium.Core.Entities;

public class PricingPlan
{
    
    public int PlanId { get; set; }
    public int HostelId { get; set; }
    public int RoomTypeId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public int DurationMonths { get; set; }
    public decimal BaseRent { get; set; }
    public decimal DiscountPercent { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public decimal FinalRent => BaseRent * DurationMonths * (1 - DiscountPercent / 100);

    public Hostel Hostel { get; set; } = null!;
    public RoomType RoomType { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

public class Booking
{
    
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public int RoomId { get; set; }
    public int? PlanId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime? CheckOutDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal FinalAmount { get; set; }
    public string BookingStatus { get; set; } = "Pending"; // Pending,Confirmed,Rejected,Cancelled,CheckedOut
    public string PaymentStatus { get; set; } = "Unpaid";  // Unpaid,Paid,Refunded
    public string? AdminNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public Room Room { get; set; } = null!;
    public PricingPlan? Plan { get; set; }
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

public class Payment
{
   
    public int PaymentId { get; set; }
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentGateway { get; set; } // Stripe, PayU
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public string PaymentStatus { get; set; } = string.Empty; // Success,Failed,Pending,Refunded
    public string? PaymentMethod { get; set; }
    public string? GatewayResponse { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Booking Booking { get; set; } = null!;
    public User User { get; set; } = null!;
}

public class Enquiry
{
  
    public int EnquiryId { get; set; }
    public int UserId { get; set; }
    public int? HostelId { get; set; }
    public string? Subject { get; set; }
    public string? Message { get; set; }
    public string EnquiryStatus { get; set; } = "New"; // New,InProgress,Resolved,Closed
    public string? AdminNotes { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public Hostel? Hostel { get; set; }
}

public class SharedArea
{
  
    public int SharedAreaId { get; set; }
    public int HostelId { get; set; }
    public string AreaName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Hostel Hostel { get; set; } = null!;
    public ICollection<SharedAreaImage> Images { get; set; } = new List<SharedAreaImage>();
}

public class SharedAreaImage
{
  
    public int ImageId { get; set; }
    public int SharedAreaId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; } = 0;

    public SharedArea SharedArea { get; set; } = null!;
}

public class Brochure
{
 
    public int BrochureId { get; set; }
    public int HostelId { get; set; }
    public string BrochureName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long? FileSize { get; set; }
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public Hostel Hostel { get; set; } = null!;
}

public class Notification
{
    
    public int NotificationId { get; set; }
    public int? UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? NotificationType { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}

public class RefreshToken
{
    
    public int TokenId { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}

public class AuditLog
{
    
    public int LogId { get; set; }
    public int? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? TableName { get; set; }
    public int? RecordId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IPAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
