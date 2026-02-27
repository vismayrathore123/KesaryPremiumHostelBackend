using KesarPremium.Core.DTOs.Request;
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
    public class HostelRepository : Repository<Hostel>, IHostelRepository
    {
        public HostelRepository(ApplicationDbContext db) : base(db) { }

        public async Task<PagedResponse<HostelListDto>> SearchAsync(HostelSearchRequest req)
        {
            var query = _db.Hostels
                .Include(h => h.Location)
                .Include(h => h.Category)
                .Include(h => h.Rooms.Where(r => r.IsActive))
                .Include(h => h.Images)
                .Where(h => h.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(req.SearchTerm))
                query = query.Where(h => h.HostelName.Contains(req.SearchTerm) || h.Location.LocationName.Contains(req.SearchTerm));
            if (req.CategoryId.HasValue)
                query = query.Where(h => h.CategoryId == req.CategoryId.Value);
            if (req.LocationId.HasValue)
                query = query.Where(h => h.LocationId == req.LocationId.Value);
            if (req.MinRent.HasValue)
                query = query.Where(h => h.Rooms.Any(r => r.MonthlyRent >= req.MinRent.Value));
            if (req.MaxRent.HasValue)
                query = query.Where(h => h.Rooms.Any(r => r.MonthlyRent <= req.MaxRent.Value));

            var total = await query.CountAsync();
            var items = await query
                .Skip((req.PageNumber - 1) * req.PageSize)
                .Take(req.PageSize)
                .Select(h => new HostelListDto
                {
                    HostelId = h.HostelId,
                    HostelName = h.HostelName,
                    Description = h.Description,
                    CategoryName = h.Category.CategoryName,
                    LocationName = h.Location.LocationName,
                    Address = h.Location.Address,
                    PinCode = h.Location.PinCode,
                    TotalAvailableBeds = h.Rooms.Sum(r => r.AvailableBeds),
                    StartingRentFrom = h.Rooms.Any() ? h.Rooms.Min(r => r.MonthlyRent) : (decimal?)null,
                    PrimaryImage = h.Images.Where(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault()
                                         ?? h.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).FirstOrDefault(),
                    ContactNumber = h.ContactNumber,
                    WhatsAppNumber = h.WhatsAppNumber
                }).ToListAsync();

            return new PagedResponse<HostelListDto>
            {
                Items = items,
                TotalCount = total,
                PageNumber = req.PageNumber,
                PageSize = req.PageSize
            };
        }

        public async Task<HostelDetailDto?> GetDetailAsync(int hostelId)
        {
            var hostel = await _db.Hostels
                .Include(h => h.Location)
                .Include(h => h.Category)
                .Include(h => h.Rooms.Where(r => r.IsActive)).ThenInclude(r => r.RoomType)
                .Include(h => h.Rooms).ThenInclude(r => r.Images)
                .Include(h => h.Images)
                .Include(h => h.HostelFacilities).ThenInclude(hf => hf.Facility)
                .Include(h => h.SharedAreas.Where(s => s.IsActive)).ThenInclude(s => s.Images)
                .Include(h => h.PricingPlans.Where(p => p.IsActive)).ThenInclude(p => p.RoomType)
                .Include(h => h.Brochures.Where(b => b.IsActive))
                .Where(h => h.HostelId == hostelId && h.IsActive)
                .FirstOrDefaultAsync();

            if (hostel == null) return null;

            return new HostelDetailDto
            {
                HostelId = hostel.HostelId,
                HostelName = hostel.HostelName,
                Description = hostel.Description,
                CategoryName = hostel.Category.CategoryName,
                LocationName = hostel.Location.LocationName,
                Address = hostel.Location.Address,
                PinCode = hostel.Location.PinCode,
                TotalRooms = hostel.TotalRooms,
                TotalBeds = hostel.TotalBeds,
                TotalAvailableBeds = hostel.Rooms.Sum(r => r.AvailableBeds),
                StartingRentFrom = hostel.Rooms.Any() ? hostel.Rooms.Min(r => r.MonthlyRent) : null,
                PrimaryImage = hostel.Images.Where(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault(),
                ContactNumber = hostel.ContactNumber,
                WhatsAppNumber = hostel.WhatsAppNumber,
                Facilities = hostel.HostelFacilities.Select(hf => new FacilityDto
                {
                    FacilityId = hf.Facility.FacilityId,
                    FacilityName = hf.Facility.FacilityName,
                    IconClass = hf.Facility.IconClass
                }).ToList(),
                Images = hostel.Images.OrderBy(i => i.DisplayOrder).Select(i => new HostelImageDto
                {
                    ImageId = i.ImageId,
                    ImageUrl = i.ImageUrl,
                    AltText = i.AltText,
                    IsPrimary = i.IsPrimary,
                    DisplayOrder = i.DisplayOrder,
                    RoomId = i.RoomId
                }).ToList(),
                Rooms = hostel.Rooms.Select(r => new RoomDto
                {
                    RoomId = r.RoomId,
                    RoomNumber = r.RoomNumber,
                    FloorNumber = r.FloorNumber,
                    TotalBeds = r.TotalBeds,
                    AvailableBeds = r.AvailableBeds,
                    MonthlyRent = r.MonthlyRent,
                    RoomTypeName = r.RoomType.TypeName,
                    Images = r.Images.Select(i => new HostelImageDto
                    { ImageId = i.ImageId, ImageUrl = i.ImageUrl, AltText = i.AltText }).ToList()
                }).ToList(),
                SharedAreas = hostel.SharedAreas.Select(s => new SharedAreaDto
                {
                    SharedAreaId = s.SharedAreaId,
                    AreaName = s.AreaName,
                    Description = s.Description,
                    Images = s.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToList()
                }).ToList(),
                PricingPlans = hostel.PricingPlans.Select(p => new PricingPlanDto
                {
                    PlanId = p.PlanId,
                    PlanName = p.PlanName,
                    DurationMonths = p.DurationMonths,
                    BaseRent = p.BaseRent,
                    DiscountPercent = p.DiscountPercent,
                    FinalRent = p.FinalRent,
                    RoomTypeName = p.RoomType.TypeName
                }).ToList(),
                LatestBrochure = hostel.Brochures.OrderByDescending(b => b.Version).Select(b => new BrochureDto
                {
                    BrochureId = b.BrochureId,
                    BrochureName = b.BrochureName,
                    FilePath = b.FilePath,
                    Version = b.Version,
                    GeneratedAt = b.GeneratedAt
                }).FirstOrDefault()
            };
        }

        public async Task<List<HostelListDto>> GetByCategoryAsync(string categoryName)
        {
            return await _db.Hostels
                .Include(h => h.Location).Include(h => h.Category)
                .Include(h => h.Rooms.Where(r => r.IsActive)).Include(h => h.Images)
                .Where(h => h.IsActive && h.Category.CategoryName == categoryName)
                .Select(h => new HostelListDto
                {
                    HostelId = h.HostelId,
                    HostelName = h.HostelName,
                    Description = h.Description,
                    CategoryName = h.Category.CategoryName,
                    LocationName = h.Location.LocationName,
                    Address = h.Location.Address,
                    TotalAvailableBeds = h.Rooms.Sum(r => r.AvailableBeds),
                    StartingRentFrom = h.Rooms.Any() ? h.Rooms.Min(r => r.MonthlyRent) : null,
                    PrimaryImage = h.Images.Where(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault()
                }).ToListAsync();
        }

        public async Task<List<HostelListDto>> GetByLocationAsync(int locationId)
        {
            return await _db.Hostels
                .Include(h => h.Location).Include(h => h.Category)
                .Include(h => h.Rooms.Where(r => r.IsActive)).Include(h => h.Images)
                .Where(h => h.IsActive && h.LocationId == locationId)
                .Select(h => new HostelListDto
                {
                    HostelId = h.HostelId,
                    HostelName = h.HostelName,
                    Description = h.Description,
                    CategoryName = h.Category.CategoryName,
                    LocationName = h.Location.LocationName,
                    StartingRentFrom = h.Rooms.Any() ? h.Rooms.Min(r => r.MonthlyRent) : null,
                    TotalAvailableBeds = h.Rooms.Sum(r => r.AvailableBeds),
                    PrimaryImage = h.Images.Where(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault()
                }).ToListAsync();
        }
    }
}
