using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayManagementApi.Interfaces;

namespace RailwayManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPayment _paymentService;

        public PaymentController(IPayment paymentService)
        {
            _paymentService = paymentService;
        }

        [Authorize]
        [HttpPost("create-order")]
        public IActionResult CreateOrder([FromQuery] decimal amount)
        {
            return _paymentService.CreateOrder(amount);
        }
    }
}
