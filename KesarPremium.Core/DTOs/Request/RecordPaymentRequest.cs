using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Core.DTOs.Request
{
    public class RecordPaymentRequest
    {
        public int BookingId { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string PaymentGateway { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string? PaymentMethod { get; set; }
        public string? GatewayResponse { get; set; }
    }
}
