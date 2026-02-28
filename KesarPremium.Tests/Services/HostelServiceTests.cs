using KesarPremium.Core.DTOs.Request;
using KesarPremium.Core.DTOs.Response;
using KesarPremium.Core.Entities;
using KesarPremium.Core.Interfaces;
using KesarPremium.Infrastructure.Data;
using KesarPremium.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace KesarPremium.Tests.Services;

public class HostelServiceTests
{
    private readonly Mock<IHostelRepository> _hostelRepoMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly ApplicationDbContext _db;
    private readonly HostelService _hostelService;

    public HostelServiceTests()
    {
        _hostelRepoMock = new Mock<IHostelRepository>();
        _cacheMock = new Mock<ICacheService>();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(options);

        _hostelService = new HostelService(_hostelRepoMock.Object, _cacheMock.Object, _db);
    }

    // ── SEARCH ────────────────────────────────────────
    [Fact]
    public async Task Search_WithNoFilters_ShouldReturnPagedResults()
    {
        // Arrange
        var pagedResult = new PagedResponse<HostelListDto>
        {
            Items = new List<HostelListDto>
            {
                new() { HostelId = 1, HostelName = "Kesar Boys Hostel", CategoryName = "Boys" },
                new() { HostelId = 2, HostelName = "Kesar Girls Hostel", CategoryName = "Girls" }
            },
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };

        _cacheMock.Setup(c => c.GetAsync<PagedResponse<HostelListDto>>(It.IsAny<string>()))
            .ReturnsAsync((PagedResponse<HostelListDto>?)null);
        _hostelRepoMock.Setup(r => r.SearchAsync(It.IsAny<HostelSearchRequest>()))
            .ReturnsAsync(pagedResult);
        _cacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<PagedResponse<HostelListDto>>(), It.IsAny<TimeSpan?>()))
            .Returns(Task.CompletedTask);

        var request = new HostelSearchRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _hostelService.SearchAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.Items.Count);
        Assert.Equal(2, result.Data.TotalCount);
    }

    [Fact]
    public async Task Search_WhenCacheHit_ShouldReturnCachedData()
    {
        // Arrange
        var cached = new PagedResponse<HostelListDto>
        {
            Items = new List<HostelListDto> { new() { HostelId = 1, HostelName = "Cached Hostel" } },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };

        _cacheMock.Setup(c => c.GetAsync<PagedResponse<HostelListDto>>(It.IsAny<string>()))
            .ReturnsAsync(cached);

        // Act
        var result = await _hostelService.SearchAsync(new HostelSearchRequest());

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Cached Hostel", result.Data!.Items[0].HostelName);
        // Repo should NOT be called since cache hit
        _hostelRepoMock.Verify(r => r.SearchAsync(It.IsAny<HostelSearchRequest>()), Times.Never);
    }

    // ── GET DETAIL ────────────────────────────────────
    [Fact]
    public async Task GetDetail_WithValidId_ShouldReturnHostelDetail()
    {
        // Arrange
        var detail = new HostelDetailDto
        {
            HostelId = 1,
            HostelName = "Kesar Boys Hostel",
            CategoryName = "Boys",
            LocationName = "Vijay Nagar",
            Facilities = new List<FacilityDto>
            {
                new() { FacilityId = 1, FacilityName = "WiFi" },
                new() { FacilityId = 2, FacilityName = "RO Water" }
            },
            Rooms = new List<RoomDto>
            {
                new() { RoomId = 1, RoomNumber = "101", AvailableBeds = 2, MonthlyRent = 5000 }
            }
        };

        _cacheMock.Setup(c => c.GetAsync<HostelDetailDto>(It.IsAny<string>()))
            .ReturnsAsync((HostelDetailDto?)null);
        _hostelRepoMock.Setup(r => r.GetDetailAsync(1)).ReturnsAsync(detail);
        _cacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<HostelDetailDto>(), It.IsAny<TimeSpan?>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _hostelService.GetDetailAsync(1);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Kesar Boys Hostel", result.Data!.HostelName);
        Assert.Equal(2, result.Data.Facilities.Count);
        Assert.Single(result.Data.Rooms);
    }

    [Fact]
    public async Task GetDetail_WithInvalidId_ShouldReturnFailure()
    {
        // Arrange
        _cacheMock.Setup(c => c.GetAsync<HostelDetailDto>(It.IsAny<string>()))
            .ReturnsAsync((HostelDetailDto?)null);
        _hostelRepoMock.Setup(r => r.GetDetailAsync(999)).ReturnsAsync((HostelDetailDto?)null);

        // Act
        var result = await _hostelService.GetDetailAsync(999);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not found", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ── GET BY CATEGORY ───────────────────────────────
    [Fact]
    public async Task GetByCategory_Boys_ShouldReturnOnlyBoysHostels()
    {
        // Arrange
        var boysHostels = new List<HostelListDto>
        {
            new() { HostelId = 1, HostelName = "Boys Hostel 1", CategoryName = "Boys" },
            new() { HostelId = 2, HostelName = "Boys Hostel 2", CategoryName = "Boys" }
        };

        _cacheMock.Setup(c => c.GetAsync<List<HostelListDto>>(It.IsAny<string>()))
            .ReturnsAsync((List<HostelListDto>?)null);
        _hostelRepoMock.Setup(r => r.GetByCategoryAsync("Boys")).ReturnsAsync(boysHostels);
        _cacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<List<HostelListDto>>(), It.IsAny<TimeSpan?>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _hostelService.GetByCategoryAsync("Boys");

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.Count);
        Assert.All(result.Data, h => Assert.Equal("Boys", h.CategoryName));
    }

    // ── CREATE HOSTEL ─────────────────────────────────
    [Fact]
    public async Task CreateHostel_WithValidData_ShouldReturnCreatedHostel()
    {
        // Arrange: seed location + category in in-memory db
        _db.Locations.Add(new Location { LocationId = 1, LocationName = "Vijay Nagar", City = "Indore" });
        _db.HostelCategories.Add(new HostelCategory { CategoryId = 1, CategoryName = "Boys" });
        _db.Facilities.Add(new Facility { FacilityId = 1, FacilityName = "WiFi" });
        await _db.SaveChangesAsync();

        var newHostel = new Hostel
        {
            HostelId = 1,
            HostelName = "New Boys Hostel",
            LocationId = 1,
            CategoryId = 1
        };

        _hostelRepoMock.Setup(r => r.AddAsync(It.IsAny<Hostel>())).ReturnsAsync(newHostel);
        _hostelRepoMock.Setup(r => r.GetDetailAsync(It.IsAny<int>())).ReturnsAsync(new HostelDetailDto
        {
            HostelId = 1,
            HostelName = "New Boys Hostel",
            CategoryName = "Boys",
            LocationName = "Vijay Nagar"
        });
        _cacheMock.Setup(c => c.RemoveByPatternAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        var request = new CreateHostelRequest
        {
            HostelName = "New Boys Hostel",
            LocationId = 1,
            CategoryId = 1,
            FacilityIds = new List<int> { 1 }
        };

        // Act
        var result = await _hostelService.CreateAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("New Boys Hostel", result.Data!.HostelName);
    }

    // ── DELETE HOSTEL ─────────────────────────────────
    [Fact]
    public async Task DeleteHostel_WithValidId_ShouldDeactivateHostel()
    {
        // Arrange
        var hostel = new Hostel { HostelId = 1, HostelName = "Test", IsActive = true };
        _hostelRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(hostel);
        _hostelRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Hostel>())).Returns(Task.CompletedTask);
        _cacheMock.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        _cacheMock.Setup(c => c.RemoveByPatternAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        // Act
        var result = await _hostelService.DeleteAsync(1);

        // Assert
        Assert.True(result.Success);
        Assert.False(hostel.IsActive);  // should be deactivated, not deleted
    }

    [Fact]
    public async Task DeleteHostel_WithInvalidId_ShouldReturnFailure()
    {
        // Arrange
        _hostelRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Hostel?)null);

        // Act
        var result = await _hostelService.DeleteAsync(999);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not found", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose() => _db.Dispose();
}
