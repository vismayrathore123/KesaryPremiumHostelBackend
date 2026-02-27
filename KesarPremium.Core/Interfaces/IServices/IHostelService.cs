using KesarPremium.Core.DTOs.Request;
using KesarPremium.Core.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.Interfaces.IServices
{
    public interface IHostelService
    {
        Task<ApiResponse<PagedResponse<HostelListDto>>> SearchAsync(HostelSearchRequest request);
        Task<ApiResponse<HostelDetailDto>> GetDetailAsync(int hostelId);
        Task<ApiResponse<List<HostelListDto>>> GetByCategoryAsync(string category);
        Task<ApiResponse<List<HostelListDto>>> GetByLocationAsync(int locationId);
        Task<ApiResponse<HostelDetailDto>> CreateAsync(CreateHostelRequest request);
        Task<ApiResponse<HostelDetailDto>> UpdateAsync(UpdateHostelRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int hostelId);
        void InvalidateCache(int hostelId);
    }
}
