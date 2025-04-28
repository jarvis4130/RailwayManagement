using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailwayManagementApi.Data;
using RailwayManagementApi.DTOs;
using RailwayManagementApi.DTOs.PassengerDTO;
using RailwayManagementApi.DTOs.TicketDTO;
using RailwayManagementApi.Helper;
using RailwayManagementApi.Interfaces;
using RailwayManagementApi.Models;
using Razorpay.Api;

namespace RailwayManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly RailwayContext _dbContext;
        private readonly INotificationService _notificationService;

        public TicketController(ITicketService ticketService, RailwayContext context, INotificationService notificationService)
        {
            _ticketService = ticketService;
            _dbContext = context;
            _notificationService = notificationService;
        }

        // {
        //   "username": "atharvaadam",
        //   "trainID": 1,
        //   "sourceID": 1,
        //   "destinationID": 2,
        //   "journeyDate": "2025-05-01",
        //   "classTypeID": 1,
        //   "passengers": [
        //     {
        //       "name": "Atharva",
        //       "gender": "Male",
        //       "age": 21
        //     }
        //   ],
        //   "paymentMode": "RazorPay",
        //   "hasInsurance": true
        // }
        [Authorize]
        [HttpPost("initiate")]
        public async Task<IActionResult> InitiateBookingAsync([FromBody] TicketBookingRequestDTO booking)
            => await _ticketService.InitiateBookingAsync(booking);

        // {
        //   "userId": "15cfd0b4-ef8a-4311-b77d-bbd81866ba18",
        //   "trainID": 1,
        //   "sourceID": 1,
        //   "destinationID": 2,
        //   "journeyDate": "2025-05-01",
        //   "classTypeID": 2,
        //   "passengers": [
        //     {
        //       "name": "Atharva",
        //       "gender": "Male",
        //       "age": 22
        //     }
        //   ],
        //   "paymentMode": "RazorPay",
        //   "paidAmount": 286,
        //   "hasInsurance": true
        // }
        [Authorize]
        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmBookingAsync([FromBody] TicketBookingConfirmDTO booking)
            => await _ticketService.ConfirmBookingAsync(booking);

        [Authorize]
        [HttpPost("confirm-payment")]
        public async Task<IActionResult> ConfirmPaymentAndBookTicket([FromBody] PaymentConfirmationDTO paymentDto)
            => await _ticketService.ConfirmPaymentAndBookTicket(paymentDto);

        [HttpPost("cancel-passenger")]
        public async Task<IActionResult> CancelPassenger([FromBody] CancelPassengerDTO dto)
        {
            return await _ticketService.CancelPassengerAsync(dto);
        }

        // [HttpGet("test")]
        // public async Task<IActionResult> SendEmail()
        // {
        //     string toEmail = "atharvaadam413@gmail.com";  // Replace with test email
        //     string subject = "Test Email from Swagger";
        //     string body = "<h2>This is a test email</h2><p>Triggered via <strong>Swagger UI</strong>.</p>";
        //     await _notificationService.SendEmailAsync(toEmail,subject,body);
        //     return Ok();
        // }


        // public async Task<WaitingListDto?> GetWaitingListForTicketAsync(int ticketId)
        // {
        //     return await _dbContext.WaitingLists
        //         .Where(w => w.TicketID == ticketId)
        //         .Select(w => new WaitingListDto
        //         {
        //             WaitingListID = w.WaitingListID,
        //             TicketID = w.TicketID,
        //             TrainID = w.TrainID,
        //             ClassTypeID = w.ClassTypeID,
        //             RequestDate = w.RequestDate,
        //             Position = w.Position
        //         })
        //         .FirstOrDefaultAsync();
        // }

    }
}

// Frontend: Initiate → Create-Order → Razorpay Checkout
//            ↓
// Razorpay: Payment Success → Frontend Receives Payment IDs
//            ↓
// Frontend: Calls /api/Ticket/confirm-payment with IDs + Booking Info
//            ↓
// Backend: Validates Signature → Finalizes Ticket Booking
