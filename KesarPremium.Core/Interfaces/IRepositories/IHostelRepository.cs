using KesarPremium.Core.DTOs.Request;
using KesarPremium.Core.DTOs.Response;
using KesarPremium.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.Interfaces.IRepositories
{
    public interface IHostelRepository : IRepository<Hostel>
    {
        Task<PagedResponse<HostelListDto>> SearchAsync(HostelSearchRequest request);
        Task<HostelDetailDto?> GetDetailAsync(int hostelId);
        Task<List<HostelListDto>> GetByCategoryAsync(string categoryName);
        Task<List<HostelListDto>> GetByLocationAsync(int locationId);
    }
}
