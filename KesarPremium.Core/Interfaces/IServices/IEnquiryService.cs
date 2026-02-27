using KesarPremium.Core.DTOs.Request;
using KesarPremium.Core.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.Interfaces.IServices
{
    public interface IEnquiryService
    {
        Task<ApiResponse<bool>> CreateAsync(int userId, CreateEnquiryRequest request);
        Task<ApiResponse<List<EnquiryDto>>> GetAdminEnquiriesAsync(string? status);
        Task<ApiResponse<List<EnquiryDto>>> GetUserEnquiriesAsync(int userId);
        Task<ApiResponse<bool>> UpdateAsync(UpdateEnquiryRequest request);
    }
}
