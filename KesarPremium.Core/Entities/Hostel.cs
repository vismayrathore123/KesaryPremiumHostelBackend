namespace KesarPremium.Core.Entities;

public class Location
{
    public int LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string City { get; set; } = "Indore";
    public string State { get; set; } = "Madhya Pradesh";
    public string? PinCode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Hostel> Hostels { get; set; } = new List<Hostel>();
}

public class HostelCategory
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty; // Boys, Girls, Independent
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Hostel> Hostels { get; set; } = new List<Hostel>();
}

public class Hostel
{
    public int HostelId { get; set; }
    public string HostelName { get; set; } = string.Empty;
    public int LocationId { get; set; }
    public int CategoryId { get; set; }
    public string? Description { get; set; }
    public string? ContactNumber { get; set; }
    public string? WhatsAppNumber { get; set; }
    public int TotalRooms { get; set; } = 0;
    public int TotalBeds { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Location Location { get; set; } = null!;
    public HostelCategory Category { get; set; } = null!;
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
    public ICollection<HostelImage> Images { get; set; } = new List<HostelImage>();
    public ICollection<HostelFacility> HostelFacilities { get; set; } = new List<HostelFacility>();
    public ICollection<PricingPlan> PricingPlans { get; set; } = new List<PricingPlan>();
    public ICollection<SharedArea> SharedAreas { get; set; } = new List<SharedArea>();
    public ICollection<Brochure> Brochures { get; set; } = new List<Brochure>();
    public ICollection<Enquiry> Enquiries { get; set; } = new List<Enquiry>();
}

public class Facility
{
    public int FacilityId { get; set; }
    public string FacilityName { get; set; } = string.Empty;
    public string? IconClass { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<HostelFacility> HostelFacilities { get; set; } = new List<HostelFacility>();
}

public class HostelFacility
{
    public int HostelFacilityId { get; set; }
    public int HostelId { get; set; }
    public int FacilityId { get; set; }

    public Hostel Hostel { get; set; } = null!;
    public Facility Facility { get; set; } = null!;
}
