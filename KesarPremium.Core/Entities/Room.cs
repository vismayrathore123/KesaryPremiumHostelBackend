using System.ComponentModel.DataAnnotations;

namespace KesarPremium.Core.Entities;

public class RoomType
{
    public int RoomTypeId { get; set; }
    public string TypeName { get; set; } = string.Empty; // Single, Double, Triple, Dormitory
    public int BedCount { get; set; } = 1;
    public string? Description { get; set; }

    public ICollection<Room> Rooms { get; set; } = new List<Room>();
    public ICollection<PricingPlan> PricingPlans { get; set; } = new List<PricingPlan>();
}

public class Room
{
    public int RoomId { get; set; }
    public int HostelId { get; set; }
    public int RoomTypeId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int FloorNumber { get; set; } = 0;
    public int TotalBeds { get; set; }
    public int AvailableBeds { get; set; }
    public decimal MonthlyRent { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Hostel Hostel { get; set; } = null!;
    public RoomType RoomType { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<HostelImage> Images { get; set; } = new List<HostelImage>();
}

public class HostelImage
{
    [Key]
    public int ImageId { get; set; }
    public int HostelId { get; set; }
    public int? RoomId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool IsPrimary { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Hostel Hostel { get; set; } = null!;
    public Room? Room { get; set; }
}
