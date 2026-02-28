using KesarPremium.Core.DTOs.Request;
using KesarPremium.Core.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KesarPremium.API.Controllers
{
    [Route("api/payments")]
    public class PaymentController : BaseController
    {
        private readonly IPaymentService _paymentService;
        public PaymentController(IPaymentService paymentService) => _paymentService = paymentService;

        [HttpPost("stripe/intent")]
        [Authorize]
        public async Task<IActionResult> CreateStripeIntent([FromBody] CreatePaymentIntentRequest request)
        {
            var result = await _paymentService.CreateStripeIntentAsync(request.BookingId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("stripe/webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var payload = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var signature = Request.Headers["Stripe-Signature"].ToString();
            var result = await _paymentService.HandleStripeWebhookAsync(payload, signature);
            return result.Success ? Ok() : BadRequest();
        }

        [HttpGet("payu/hash/{bookingId}")]
        [Authorize]
        public async Task<IActionResult> GetPayUHash(int bookingId)
        {
            var result = await _paymentService.GeneratePayUHashAsync(bookingId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("record")]
        [Authorize]
        public async Task<IActionResult> Record([FromBody] RecordPaymentRequest request)
        {
            var result = await _paymentService.RecordPaymentAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
