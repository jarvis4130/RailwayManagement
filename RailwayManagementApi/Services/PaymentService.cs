using Microsoft.AspNetCore.Mvc;
using Razorpay.Api;
using RailwayManagementApi.Interfaces;
using Newtonsoft.Json;

namespace RailwayManagementApi.Services
{
    public class PaymentService : IPayment
    {
        private readonly IConfiguration _configuration;

        public PaymentService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult CreateOrder(decimal amount)
        {
            string key =  _configuration["Razorpay:Key"];
            string secret =  _configuration["Razorpay:Secret"];

            // Console.WriteLine(key + " " + secret);

            RazorpayClient client = new RazorpayClient(key, secret);

            Dictionary<string, object> options = new()
            {
                { "amount", (int)(amount * 100) }, // amount in paise 
                { "currency", "INR" },
                { "receipt", $"rcpt_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}" },
                { "payment_capture", 1 }
            };

            // Console.WriteLine(JsonConvert.SerializeObject(options, Formatting.Indented));

            Order order = client.Order.Create(options);

            return new OkObjectResult(new
            {
                orderId = order["id"].ToString(),
                amount = amount * 100,
                currency = "INR"
            });
        }
    }
}
