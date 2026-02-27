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
    public class PaymentRepository : Repository<Payment>, IPaymentRepository
    {
        public PaymentRepository(ApplicationDbContext db) : base(db) { }

        public async Task<Payment?> GetByBookingAsync(int bookingId)
            => await _db.Payments.Where(p => p.BookingId == bookingId && p.PaymentStatus == "Success").FirstOrDefaultAsync();

        public async Task<Payment?> GetByTransactionIdAsync(string transactionId)
            => await _db.Payments.FirstOrDefaultAsync(p => p.TransactionId == transactionId);
    }
}
