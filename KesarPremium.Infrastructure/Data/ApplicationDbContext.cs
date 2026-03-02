using KesarPremium.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace KesarPremium.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<HostelCategory> HostelCategories => Set<HostelCategory>();
    public DbSet<Hostel> Hostels => Set<Hostel>();
    public DbSet<Facility> Facilities => Set<Facility>();
    public DbSet<HostelFacility> HostelFacilities => Set<HostelFacility>();
    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<HostelImage> HostelImages => Set<HostelImage>();
    public DbSet<PricingPlan> PricingPlans => Set<PricingPlan>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Enquiry> Enquiries => Set<Enquiry>();
    public DbSet<SharedArea> SharedAreas => Set<SharedArea>();
    public DbSet<SharedAreaImage> SharedAreaImages => Set<SharedAreaImage>();
    public DbSet<Brochure> Brochures => Set<Brochure>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Roles seed
        modelBuilder.Entity<Role>().HasData(
            new Role { RoleId = 1, RoleName = "Admin" },
            new Role { RoleId = 2, RoleName = "User" }
        );

        // HostelCategory seed
        modelBuilder.Entity<HostelCategory>().HasData(
            new HostelCategory { CategoryId = 1, CategoryName = "Boys", Description = "Hostel for male residents" },
            new HostelCategory { CategoryId = 2, CategoryName = "Girls", Description = "Hostel for female residents" },
            new HostelCategory { CategoryId = 3, CategoryName = "Independent", Description = "Private rooms for both" }
        );

        // RoomType seed
        modelBuilder.Entity<RoomType>().HasData(
            new RoomType { RoomTypeId = 1, TypeName = "Single", BedCount = 1 },
            new RoomType { RoomTypeId = 2, TypeName = "Double", BedCount = 2 },
            new RoomType { RoomTypeId = 3, TypeName = "Triple", BedCount = 3 },
            new RoomType { RoomTypeId = 4, TypeName = "Dormitory", BedCount = 6 }
        );

        // Facilities seed
        modelBuilder.Entity<Facility>().HasData(
            new Facility { FacilityId = 1, FacilityName = "Room Cleaning", IconClass = "fas fa-broom" },
            new Facility { FacilityId = 2, FacilityName = "Toilet Cleaning", IconClass = "fas fa-toilet" },
            new Facility { FacilityId = 3, FacilityName = "Electricity", IconClass = "fas fa-bolt" },
            new Facility { FacilityId = 4, FacilityName = "Security", IconClass = "fas fa-shield-alt" },
            new Facility { FacilityId = 5, FacilityName = "RO Water", IconClass = "fas fa-tint" },
            new Facility { FacilityId = 6, FacilityName = "Vehicle Parking", IconClass = "fas fa-parking" },
            new Facility { FacilityId = 7, FacilityName = "CCTV Cameras", IconClass = "fas fa-video" },
            new Facility { FacilityId = 8, FacilityName = "WiFi", IconClass = "fas fa-wifi" },
            new Facility { FacilityId = 9, FacilityName = "Breakfast", IconClass = "fas fa-coffee" },
            new Facility { FacilityId = 10, FacilityName = "Lunch", IconClass = "fas fa-utensils" },
            new Facility { FacilityId = 11, FacilityName = "Snacks", IconClass = "fas fa-cookie" },
            new Facility { FacilityId = 12, FacilityName = "Dinner", IconClass = "fas fa-hamburger" }
        );

        // Locations seed
        modelBuilder.Entity<Location>().HasData(
            new Location { LocationId = 1, LocationName = "Vijay Nagar", Address = "Vijay Nagar, Indore", PinCode = "452010" },
            new Location { LocationId = 2, LocationName = "Palasia", Address = "Palasia, Indore", PinCode = "452001" },
            new Location { LocationId = 3, LocationName = "Bhawarkua", Address = "Bhawarkua, Indore", PinCode = "452001" },
            new Location { LocationId = 4, LocationName = "Scheme 54", Address = "Scheme 54, Indore", PinCode = "452010" },
            new Location { LocationId = 5, LocationName = "AB Road", Address = "AB Road, Indore", PinCode = "452008" }
        );

        // Decimal precision
        modelBuilder.Entity<PricingPlan>()
            .Property(p => p.BaseRent).HasColumnType("decimal(10,2)");
        modelBuilder.Entity<PricingPlan>()
            .Property(p => p.DiscountPercent).HasColumnType("decimal(5,2)");
        modelBuilder.Entity<Room>()
            .Property(r => r.MonthlyRent).HasColumnType("decimal(10,2)");
        modelBuilder.Entity<Booking>()
            .Property(b => b.TotalAmount).HasColumnType("decimal(10,2)");
        modelBuilder.Entity<Booking>()
            .Property(b => b.DiscountAmount).HasColumnType("decimal(10,2)");
        modelBuilder.Entity<Booking>()
            .Property(b => b.FinalAmount).HasColumnType("decimal(10,2)");
        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount).HasColumnType("decimal(10,2)");

        // HostelFacility unique constraint
        modelBuilder.Entity<HostelFacility>()
            .HasIndex(hf => new { hf.HostelId, hf.FacilityId }).IsUnique();

        // PricingPlan computed column ignored (handled in C#)
        modelBuilder.Entity<PricingPlan>()
            .Ignore(p => p.FinalRent);

        // ── FIX: Prevent cascade delete cycles ──────────────────────────────
        // SQL Server does not allow multiple cascade paths to the same table.
        // All relationships below use NoAction so SQL Server doesn't complain.
        // EF Core / application code handles cleanup instead.

        // User → Bookings
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // User → Payments (via Booking → User creates a cycle)
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // Booking → Payments
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Booking)
            .WithMany(b => b.Payments)
            .HasForeignKey(p => p.BookingId)
            .OnDelete(DeleteBehavior.NoAction);

        // User → Enquiries
        modelBuilder.Entity<Enquiry>()
            .HasOne(e => e.User)
            .WithMany(u => u.Enquiries)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // User → Notifications
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // User → RefreshTokens
        modelBuilder.Entity<RefreshToken>()
            .HasOne(r => r.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // ── Explicit Primary Keys (non-convention names) ─────────────────────
        // EF Core convention only auto-detects "Id" or "ClassNameId".
        // Everything else must be declared explicitly here.

        modelBuilder.Entity<AuditLog>()
            .HasKey(a => a.LogId);

        modelBuilder.Entity<SharedAreaImage>()
            .HasKey(s => s.ImageId);

        modelBuilder.Entity<HostelImage>()
            .HasKey(i => i.ImageId);

        modelBuilder.Entity<RefreshToken>()
            .HasKey(r => r.TokenId);

        modelBuilder.Entity<Location>()
            .HasKey(l => l.LocationId);

        modelBuilder.Entity<HostelCategory>()
            .HasKey(c => c.CategoryId);

        modelBuilder.Entity<Role>()
            .HasKey(r => r.RoleId);

        modelBuilder.Entity<User>()
            .HasKey(u => u.UserId);

        modelBuilder.Entity<Hostel>()
            .HasKey(h => h.HostelId);

        modelBuilder.Entity<Facility>()
            .HasKey(f => f.FacilityId);

        modelBuilder.Entity<HostelFacility>()
            .HasKey(hf => hf.HostelFacilityId);

        modelBuilder.Entity<RoomType>()
            .HasKey(r => r.RoomTypeId);

        modelBuilder.Entity<Room>()
            .HasKey(r => r.RoomId);

        modelBuilder.Entity<PricingPlan>()
            .HasKey(p => p.PlanId);

        modelBuilder.Entity<Booking>()
            .HasKey(b => b.BookingId);

        modelBuilder.Entity<Payment>()
            .HasKey(p => p.PaymentId);

        modelBuilder.Entity<Enquiry>()
            .HasKey(e => e.EnquiryId);

        modelBuilder.Entity<SharedArea>()
            .HasKey(s => s.SharedAreaId);

        modelBuilder.Entity<Brochure>()
            .HasKey(b => b.BrochureId);

        modelBuilder.Entity<Notification>()
            .HasKey(n => n.NotificationId);

        // AuditLog → UserId is nullable int, no navigation property
        modelBuilder.Entity<AuditLog>()
            .Property(a => a.UserId)
            .IsRequired(false);

        // Notification → UserId is nullable
        modelBuilder.Entity<Notification>()
            .Property(n => n.UserId)
            .IsRequired(false);

        // Hostel → Rooms
        modelBuilder.Entity<Room>()
            .HasOne(r => r.Hostel)
            .WithMany(h => h.Rooms)
            .HasForeignKey(r => r.HostelId)
            .OnDelete(DeleteBehavior.NoAction);

        // Hostel → HostelImages
        modelBuilder.Entity<HostelImage>()
            .HasOne(i => i.Hostel)
            .WithMany(h => h.Images)
            .HasForeignKey(i => i.HostelId)
            .OnDelete(DeleteBehavior.NoAction);

        // Hostel → HostelFacilities
        modelBuilder.Entity<HostelFacility>()
            .HasOne(hf => hf.Hostel)
            .WithMany(h => h.HostelFacilities)
            .HasForeignKey(hf => hf.HostelId)
            .OnDelete(DeleteBehavior.NoAction);

        // Hostel → SharedAreas
        modelBuilder.Entity<SharedArea>()
            .HasOne(s => s.Hostel)
            .WithMany(h => h.SharedAreas)
            .HasForeignKey(s => s.HostelId)
            .OnDelete(DeleteBehavior.NoAction);

        // Room → Bookings
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Room)
            .WithMany(r => r.Bookings)
            .HasForeignKey(b => b.RoomId)
            .OnDelete(DeleteBehavior.NoAction);

        // Hostel → Enquiries
        modelBuilder.Entity<Enquiry>()
            .HasOne(e => e.Hostel)
            .WithMany()
            .HasForeignKey(e => e.HostelId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
