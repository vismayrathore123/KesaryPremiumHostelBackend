using KesarPremium.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.Interfaces.IRepositories
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<Payment?> GetByBookingAsync(int bookingId);
        Task<Payment?> GetByTransactionIdAsync(string transactionId);
    }
}
