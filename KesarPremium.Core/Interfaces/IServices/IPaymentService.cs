using KesarPremium.Core.DTOs.Request;
using KesarPremium.Core.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.Interfaces.IServices
{
    public interface IPaymentService
    {
        Task<ApiResponse<PaymentIntentResponse>> CreateStripeIntentAsync(int bookingId);
        Task<ApiResponse<bool>> HandleStripeWebhookAsync(string payload, string signature);
        Task<ApiResponse<string>> GeneratePayUHashAsync(int bookingId);
        Task<ApiResponse<bool>> RecordPaymentAsync(RecordPaymentRequest request);
    }

}
