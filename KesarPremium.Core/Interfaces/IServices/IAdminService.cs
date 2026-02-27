using KesarPremium.Core.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.Interfaces.IServices
{
    public interface IAdminService
    {
        Task<ApiResponse<AdminDashboardStats>> GetDashboardStatsAsync();
    }
}
