using KesarPremium.Core.DTOs.Request;
using KesarPremium.Core.DTOs.Response;
using KesarPremium.Core.Entities;
using KesarPremium.Core.Interfaces.IRepositories;
using KesarPremium.Core.Interfaces.IServices;
using KesarPremium.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Infrastructure.Services
{
    public class EnquiryService : IEnquiryService
    {

        public EnquiryService(IEnquiryRepository repo) => _repo = repo;

        public async Task<ApiResponse<bool>> CreateAsync(int userId, CreateEnquiryRequest req)
        {
            var enquiry = new Enquiry
            {
                UserId = userId,
                HostelId = req.HostelId,
                Subject = req.Subject,
                Message = req.Message,
                EnquiryStatus = "New"
            };

            await _repo.AddAsync(enquiry);
            return ApiResponse<bool>.Ok(true, "Enquiry submitted successfully.");
        }

        public async Task<ApiResponse<List<EnquiryDto>>> GetAdminEnquiriesAsync(string? status)
        {
            var list = await _repo.GetAdminEnquiriesAsync(status);
            return ApiResponse<List<EnquiryDto>>.Ok(list);
        }

        public async Task<ApiResponse<List<EnquiryDto>>> GetUserEnquiriesAsync(int userId)
        {
            var list = await _repo.GetByUserAsync(userId);
            return ApiResponse<List<EnquiryDto>>.Ok(list);
        }

        public async Task<ApiResponse<bool>> UpdateAsync(UpdateEnquiryRequest req)
        {
            var enquiry = await _repo.GetByIdAsync(req.EnquiryId);
            if (enquiry == null) return ApiResponse<bool>.Fail("Enquiry not found.");

            enquiry.EnquiryStatus = req.Status;
            enquiry.AdminNotes = req.AdminNotes;
            enquiry.FollowUpDate = req.FollowUpDate;
            enquiry.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(enquiry);
            return ApiResponse<bool>.Ok(true, "Enquiry updated.");
        }
    }
}
