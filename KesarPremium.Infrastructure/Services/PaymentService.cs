using KesarPremium.Core.DTOs.Request;
using KesarPremium.Core.DTOs.Response;
using KesarPremium.Core.Entities;
using KesarPremium.Core.Interfaces.IRepositories;
using KesarPremium.Core.Interfaces.IServices;
using KesarPremium.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly IBookingRepository _bookingRepo;
        private readonly INotificationService _notifService;
        private readonly IConfiguration _config;

        public PaymentService(IPaymentRepository paymentRepo, IBookingRepository bookingRepo,
            INotificationService notifService, IConfiguration config)
        {
            _paymentRepo = paymentRepo;
            _bookingRepo = bookingRepo;
            _notifService = notifService;
            _config = config;
        }

        public async Task<ApiResponse<PaymentIntentResponse>> CreateStripeIntentAsync(int bookingId)
        {
            StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) return ApiResponse<PaymentIntentResponse>.Fail("Booking not found.");

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(booking.FinalAmount * 100),
                Currency = "inr",
                Metadata = new Dictionary<string, string> { { "bookingId", bookingId.ToString() } }
            };

            var service = new PaymentIntentService();
            var intent = await service.CreateAsync(options);

            return ApiResponse<PaymentIntentResponse>.Ok(new PaymentIntentResponse
            {
                ClientSecret = intent.ClientSecret,
                PublishableKey = _config["Stripe:PublishableKey"]!
            });
        }

        public async Task<ApiResponse<bool>> HandleStripeWebhookAsync(string payload, string signature)
        {
            try
            {
                var webhookSecret = _config["Stripe:WebhookSecret"]!;
                var stripeEvent = EventUtility.ConstructEvent(payload, signature, webhookSecret);

                if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    var intent = (PaymentIntent)stripeEvent.Data.Object;
                    var bookingId = int.Parse(intent.Metadata["bookingId"]);
                    await RecordPaymentAsync(new RecordPaymentRequest
                    {
                        BookingId = bookingId,
                        TransactionId = intent.Id,
                        PaymentGateway = "Stripe",
                        Amount = intent.Amount / 100m,
                        PaymentStatus = "Success",
                        PaymentMethod = intent.PaymentMethodTypes?.FirstOrDefault(),
                        GatewayResponse = payload
                    });
                }

                return ApiResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Webhook error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> GeneratePayUHashAsync(int bookingId)
        {
            var booking = await _bookingRepo.GetDetailAsync(bookingId);
            if (booking == null) return ApiResponse<string>.Fail("Booking not found.");

            var key = _config["PayU:MerchantKey"]!;
            var salt = _config["PayU:Salt"]!;
            var txnId = booking.BookingNumber;
            var amount = booking.FinalAmount.ToString("F2");
            var productInfo = $"Hostel Booking - {booking.HostelName}";
            var firstName = booking.UserName;
            var email = booking.UserEmail;

            var hashString = $"{key}|{txnId}|{amount}|{productInfo}|{firstName}|{email}|||||||||||{salt}";
            using var sha256 = System.Security.Cryptography.SHA512.Create();
            var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(hashString));
            var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            return ApiResponse<string>.Ok(hash);
        }

        public async Task<ApiResponse<bool>> RecordPaymentAsync(RecordPaymentRequest req)
        {
            var booking = await _bookingRepo.GetByIdAsync(req.BookingId);
            if (booking == null) return ApiResponse<bool>.Fail("Booking not found.");

            var payment = new Payment
            {
                BookingId = req.BookingId,
                UserId = booking.UserId,
                TransactionId = req.TransactionId,
                PaymentGateway = req.PaymentGateway,
                Amount = req.Amount,
                PaymentStatus = req.PaymentStatus,
                PaymentMethod = req.PaymentMethod,
                GatewayResponse = req.GatewayResponse,
                PaidAt = req.PaymentStatus == "Success" ? DateTime.UtcNow : null
            };

            await _paymentRepo.AddAsync(payment);

            if (req.PaymentStatus == "Success")
            {
                booking.PaymentStatus = "Paid";
                booking.BookingStatus = "Confirmed";
                booking.UpdatedAt = DateTime.UtcNow;
                await _bookingRepo.UpdateAsync(booking);

                await _notifService.SendAsync(booking.UserId, "Payment Successful",
                    $"Payment of ₹{req.Amount:N2} received. Booking {booking.BookingNumber} is confirmed!", "PaymentReceived");
            }

            return ApiResponse<bool>.Ok(true, "Payment recorded.");
        }
    }
}
