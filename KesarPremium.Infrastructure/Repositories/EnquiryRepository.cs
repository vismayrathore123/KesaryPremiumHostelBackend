using KesarPremium.Core.DTOs.Response;
using KesarPremium.Core.Entities;
using KesarPremium.Core.Interfaces.IRepositories;
using KesarPremium.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Infrastructure.Repositories
{
    public class EnquiryRepository : Repository<Enquiry>, IEnquiryRepository
    {
        public EnquiryRepository(ApplicationDbContext db) : base(db) { }

        public async Task<List<EnquiryDto>> GetAdminEnquiriesAsync(string? status)
        {
            var query = _db.Enquiries.Include(e => e.User).Include(e => e.Hostel).AsQueryable();
            if (!string.IsNullOrEmpty(status)) query = query.Where(e => e.EnquiryStatus == status);
            return await query.OrderByDescending(e => e.CreatedAt).Select(e => MapDto(e)).ToListAsync();
        }

        public async Task<List<EnquiryDto>> GetByUserAsync(int userId)
            => await _db.Enquiries.Include(e => e.Hostel).Where(e => e.UserId == userId)
                .OrderByDescending(e => e.CreatedAt).Select(e => MapDto(e)).ToListAsync();

        private static EnquiryDto MapDto(Enquiry e) => new()
        {
            EnquiryId = e.EnquiryId,
            Subject = e.Subject,
            Message = e.Message,
            EnquiryStatus = e.EnquiryStatus,
            AdminNotes = e.AdminNotes,
            FollowUpDate = e.FollowUpDate,
            CreatedAt = e.CreatedAt,
            UserName = e.User?.FullName ?? "",
            UserEmail = e.User?.Email ?? "",
            UserPhone = e.User?.PhoneNumber,
            HostelName = e.Hostel?.HostelName
        };
    }

}
