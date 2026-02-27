using KesarPremium.Core.DTOs.Request;
using KesarPremium.Core.DTOs.Response;
using KesarPremium.Core.Entities;
using KesarPremium.Infrastructure.Data;
using KesarPremium.Infrastructure.Repositories;
using KesarPremium.Infrastructure.Services.AuthServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Infrastructure.Services
{
    public class HostelService : IHostelService
    {
        private readonly IHostelRepository _repo;
        private readonly ICacheService _cache;
        private readonly ApplicationDbContext _db;

        public HostelService(IHostelRepository repo, ICacheService cache, ApplicationDbContext db)
        {
            _repo = repo;
            _cache = cache;
            _db = db;
        }

        public async Task<ApiResponse<PagedResponse<HostelListDto>>> SearchAsync(HostelSearchRequest req)
        {
            var cacheKey = $"hostel:search:{req.CategoryId}:{req.LocationId}:{req.SearchTerm}:{req.PageNumber}";
            var cached = await _cache.GetAsync<PagedResponse<HostelListDto>>(cacheKey);
            if (cached != null) return ApiResponse<PagedResponse<HostelListDto>>.Ok(cached);

            var result = await _repo.SearchAsync(req);
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
            return ApiResponse<PagedResponse<HostelListDto>>.Ok(result);
        }

        public async Task<ApiResponse<HostelDetailDto>> GetDetailAsync(int hostelId)
        {
            var cacheKey = $"hostel:{hostelId}:detail";
            var cached = await _cache.GetAsync<HostelDetailDto>(cacheKey);
            if (cached != null) return ApiResponse<HostelDetailDto>.Ok(cached);

            var detail = await _repo.GetDetailAsync(hostelId);
            if (detail == null) return ApiResponse<HostelDetailDto>.Fail("Hostel not found.");

            await _cache.SetAsync(cacheKey, detail, TimeSpan.FromMinutes(5));
            return ApiResponse<HostelDetailDto>.Ok(detail);
        }

        public async Task<ApiResponse<List<HostelListDto>>> GetByCategoryAsync(string category)
        {
            var cacheKey = $"hostel:category:{category}";
            var cached = await _cache.GetAsync<List<HostelListDto>>(cacheKey);
            if (cached != null) return ApiResponse<List<HostelListDto>>.Ok(cached);

            var result = await _repo.GetByCategoryAsync(category);
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));
            return ApiResponse<List<HostelListDto>>.Ok(result);
        }

        public async Task<ApiResponse<List<HostelListDto>>> GetByLocationAsync(int locationId)
        {
            var result = await _repo.GetByLocationAsync(locationId);
            return ApiResponse<List<HostelListDto>>.Ok(result);
        }

        public async Task<ApiResponse<HostelDetailDto>> CreateAsync(CreateHostelRequest req)
        {
            var hostel = new Core.Entities.Hostel
            {
                HostelName = req.HostelName,
                LocationId = req.LocationId,
                CategoryId = req.CategoryId,
                Description = req.Description,
                ContactNumber = req.ContactNumber,
                WhatsAppNumber = req.WhatsAppNumber
            };

            foreach (var fid in req.FacilityIds)
                hostel.HostelFacilities.Add(new HostelFacility { FacilityId = fid });

            await _repo.AddAsync(hostel);
            await _cache.RemoveByPatternAsync("hostel:search");
            await _cache.RemoveByPatternAsync("hostel:category");

            var detail = await _repo.GetDetailAsync(hostel.HostelId);
            return ApiResponse<HostelDetailDto>.Ok(detail!, "Hostel created successfully.");
        }

        public async Task<ApiResponse<HostelDetailDto>> UpdateAsync(UpdateHostelRequest req)
        {
            var hostel = await _db.Hostels.Include(h => h.HostelFacilities).FirstOrDefaultAsync(h => h.HostelId == req.HostelId);
            if (hostel == null) return ApiResponse<HostelDetailDto>.Fail("Hostel not found.");

            hostel.HostelName = req.HostelName;
            hostel.LocationId = req.LocationId;
            hostel.CategoryId = req.CategoryId;
            hostel.Description = req.Description;
            hostel.ContactNumber = req.ContactNumber;
            hostel.WhatsAppNumber = req.WhatsAppNumber;
            hostel.UpdatedAt = DateTime.UtcNow;

            // Update facilities
            _db.HostelFacilities.RemoveRange(hostel.HostelFacilities);
            foreach (var fid in req.FacilityIds)
                hostel.HostelFacilities.Add(new HostelFacility { HostelId = hostel.HostelId, FacilityId = fid });

            await _db.SaveChangesAsync();
            InvalidateCache(req.HostelId);

            var detail = await _repo.GetDetailAsync(req.HostelId);
            return ApiResponse<HostelDetailDto>.Ok(detail!, "Hostel updated successfully.");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int hostelId)
        {
            var hostel = await _repo.GetByIdAsync(hostelId);
            if (hostel == null) return ApiResponse<bool>.Fail("Hostel not found.");
            hostel.IsActive = false;
            hostel.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(hostel);
            InvalidateCache(hostelId);
            return ApiResponse<bool>.Ok(true, "Hostel deactivated.");
        }

        public void InvalidateCache(int hostelId)
        {
            _cache.RemoveAsync($"hostel:{hostelId}:detail");
            _cache.RemoveByPatternAsync("hostel:search");
            _cache.RemoveByPatternAsync("hostel:category");
        }
    }
}
