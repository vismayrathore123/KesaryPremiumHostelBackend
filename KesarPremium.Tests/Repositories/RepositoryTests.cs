using KesarPremium.Core.DTOs.Request;
using KesarPremium.Core.Entities;
using KesarPremium.Infrastructure.Data;
using KesarPremium.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KesarPremium.Tests.Repositories;

/// <summary>
/// Repository tests using EF Core InMemory database.
/// These are integration-style tests that verify LINQ queries work correctly.
/// </summary>
public class RepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _db;

    public RepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(options);

        SeedTestData();
    }

    private void SeedTestData()
    {
        // Roles
        _db.Roles.AddRange(
            new Role { RoleId = 1, RoleName = "Admin" },
            new Role { RoleId = 2, RoleName = "User" }
        );

        // Locations
        _db.Locations.AddRange(
            new Location { LocationId = 1, LocationName = "Vijay Nagar", City = "Indore", PinCode = "452010" },
            new Location { LocationId = 2, LocationName = "Palasia", City = "Indore", PinCode = "452001" }
        );

        // Categories
        _db.HostelCategories.AddRange(
            new HostelCategory { CategoryId = 1, CategoryName = "Boys" },
            new HostelCategory { CategoryId = 2, CategoryName = "Girls" },
            new HostelCategory { CategoryId = 3, CategoryName = "Independent" }
        );

        // Facilities
        _db.Facilities.AddRange(
            new Facility { FacilityId = 1, FacilityName = "WiFi", IconClass = "fas fa-wifi" },
            new Facility { FacilityId = 2, FacilityName = "Security", IconClass = "fas fa-shield-alt" }
        );

        // Hostels
        _db.Hostels.AddRange(
            new Hostel { HostelId = 1, HostelName = "Kesar Boys Hostel", LocationId = 1, CategoryId = 1, IsActive = true, TotalRooms = 5, TotalBeds = 15 },
            new Hostel { HostelId = 2, HostelName = "Kesar Girls Hostel", LocationId = 1, CategoryId = 2, IsActive = true, TotalRooms = 4, TotalBeds = 12 },
            new Hostel { HostelId = 3, HostelName = "Palasia Boys", LocationId = 2, CategoryId = 1, IsActive = true, TotalRooms = 3, TotalBeds = 9 },
            new Hostel { HostelId = 4, HostelName = "Inactive Hostel", LocationId = 1, CategoryId = 1, IsActive = false }
        );

        // RoomTypes
        _db.RoomTypes.Add(new RoomType { RoomTypeId = 1, TypeName = "Double", BedCount = 2 });

        // Rooms
        _db.Rooms.AddRange(
            new Room { RoomId = 1, HostelId = 1, RoomTypeId = 1, RoomNumber = "101", TotalBeds = 2, AvailableBeds = 2, MonthlyRent = 5000m, IsActive = true },
            new Room { RoomId = 2, HostelId = 1, RoomTypeId = 1, RoomNumber = "102", TotalBeds = 2, AvailableBeds = 0, MonthlyRent = 5000m, IsActive = true },
            new Room { RoomId = 3, HostelId = 2, RoomTypeId = 1, RoomNumber = "201", TotalBeds = 2, AvailableBeds = 1, MonthlyRent = 6000m, IsActive = true }
        );

        // Users
        _db.Users.AddRange(
            new User { UserId = 1, FullName = "Test User", Email = "user@test.com", PasswordHash = "hash1", RoleId = 2, IsActive = true },
            new User { UserId = 2, FullName = "Admin User", Email = "admin@test.com", PasswordHash = "hash2", RoleId = 1, IsActive = true }
        );

        _db.SaveChanges();
    }

    // ── USER REPOSITORY ───────────────────────────────
    [Fact]
    public async Task UserRepo_GetByEmail_ShouldReturnUserWithRole()
    {
        var repo = new UserRepository(_db);
        var user = await repo.GetByEmailAsync("user@test.com");

        Assert.NotNull(user);
        Assert.Equal("Test User", user.FullName);
        Assert.NotNull(user.Role);
        Assert.Equal("User", user.Role.RoleName);
    }

    [Fact]
    public async Task UserRepo_GetByEmail_WithNonExistentEmail_ShouldReturnNull()
    {
        var repo = new UserRepository(_db);
        var user = await repo.GetByEmailAsync("nobody@test.com");
        Assert.Null(user);
    }

    [Fact]
    public async Task UserRepo_EmailExists_ShouldReturnTrue_ForExistingEmail()
    {
        var repo = new UserRepository(_db);
        var exists = await repo.EmailExistsAsync("user@test.com");
        Assert.True(exists);
    }

    [Fact]
    public async Task UserRepo_EmailExists_ShouldReturnFalse_ForNewEmail()
    {
        var repo = new UserRepository(_db);
        var exists = await repo.EmailExistsAsync("newuser@test.com");
        Assert.False(exists);
    }

    // ── ROOM REPOSITORY ───────────────────────────────
    [Fact]
    public async Task RoomRepo_GetAvailableRooms_ShouldOnlyReturnRoomsWithBeds()
    {
        var repo = new RoomRepository(_db);
        var rooms = await repo.GetAvailableRoomsAsync(hostelId: 1);

        // Room 101 has 2 available beds, Room 102 has 0 → only 1 should return
        Assert.Single(rooms);
        Assert.Equal("101", rooms[0].RoomNumber);
        Assert.True(rooms[0].AvailableBeds > 0);
    }

    [Fact]
    public async Task RoomRepo_DecreaseBed_ShouldReduceAvailableBedsBy1()
    {
        var repo = new RoomRepository(_db);
        var before = (await repo.GetByIdAsync(1))!.AvailableBeds;

        var result = await repo.DecreaseBedAsync(1);

        var after = (await repo.GetByIdAsync(1))!.AvailableBeds;

        Assert.True(result);
        Assert.Equal(before - 1, after);
    }

    [Fact]
    public async Task RoomRepo_DecreaseBed_WhenNoBedsAvailable_ShouldReturnFalse()
    {
        var repo = new RoomRepository(_db);
        var result = await repo.DecreaseBedAsync(2);  // Room 102 has 0 beds

        Assert.False(result);
    }

    [Fact]
    public async Task RoomRepo_IncreaseBed_ShouldIncrementAvailableBeds()
    {
        var repo = new RoomRepository(_db);
        var before = (await repo.GetByIdAsync(2))!.AvailableBeds; // 0 beds

        await repo.IncreaseBedAsync(2);

        var after = (await repo.GetByIdAsync(2))!.AvailableBeds;

        Assert.Equal(before + 1, after);
    }

    // ── GENERIC REPOSITORY ────────────────────────────
    [Fact]
    public async Task GenericRepo_GetById_ShouldReturnCorrectEntity()
    {
        var repo = new Repository<Hostel>(_db);
        var hostel = await repo.GetByIdAsync(1);

        Assert.NotNull(hostel);
        Assert.Equal("Kesar Boys Hostel", hostel.HostelName);
    }

    [Fact]
    public async Task GenericRepo_GetById_WithInvalidId_ShouldReturnNull()
    {
        var repo = new Repository<Hostel>(_db);
        var hostel = await repo.GetByIdAsync(9999);

        Assert.Null(hostel);
    }

    [Fact]
    public async Task GenericRepo_GetAll_ShouldReturnAllEntities()
    {
        var repo = new Repository<Hostel>(_db);
        var hostels = await repo.GetAllAsync();

        // We seeded 4 hostels (including inactive)
        Assert.Equal(4, hostels.Count());
    }

    [Fact]
    public async Task GenericRepo_Add_ShouldPersistEntity()
    {
        var repo = new Repository<Hostel>(_db);
        var hostel = new Hostel { HostelName = "New Hostel", LocationId = 1, CategoryId = 1, IsActive = true };

        await repo.AddAsync(hostel);

        var found = await repo.GetByIdAsync(hostel.HostelId);

        Assert.NotNull(found);
        Assert.Equal("New Hostel", found.HostelName);
    }

    [Fact]
    public async Task GenericRepo_Delete_ShouldRemoveEntity()
    {
        // Add a temp entity to delete
        var hostel = new Hostel { HostelName = "Temp Hostel", LocationId = 1, CategoryId = 1 };
        _db.Hostels.Add(hostel);
        await _db.SaveChangesAsync();

        var repo = new Repository<Hostel>(_db);
        await repo.DeleteAsync(hostel);

        var found = await repo.GetByIdAsync(hostel.HostelId);
        Assert.Null(found);
    }

    [Fact]
    public async Task GenericRepo_Exists_ShouldReturnTrue_ForExistingId()
    {
        var repo = new Repository<Hostel>(_db);
        var exists = await repo.ExistsAsync(1);
        Assert.True(exists);
    }

    [Fact]
    public async Task GenericRepo_Exists_ShouldReturnFalse_ForMissingId()
    {
        var repo = new Repository<Hostel>(_db);
        var exists = await repo.ExistsAsync(9999);
        Assert.False(exists);
    }

    // ── NOTIFICATION REPOSITORY ───────────────────────
    [Fact]
    public async Task NotifRepo_MarkAllRead_ShouldUpdateAllUnreadNotifications()
    {
        // Seed some notifications
        _db.Notifications.AddRange(
            new Notification { UserId = 1, Title = "Test 1", Message = "Msg 1", IsRead = false },
            new Notification { UserId = 1, Title = "Test 2", Message = "Msg 2", IsRead = false },
            new Notification { UserId = 1, Title = "Test 3", Message = "Msg 3", IsRead = true }
        );
        await _db.SaveChangesAsync();

        var repo = new NotificationRepository(_db);
        await repo.MarkAllReadAsync(userId: 1);

        var unread = await _db.Notifications.Where(n => n.UserId == 1 && !n.IsRead).CountAsync();
        Assert.Equal(0, unread);
    }

    public void Dispose() => _db.Dispose();
}
