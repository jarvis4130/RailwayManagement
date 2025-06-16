using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailwayManagementApi.Data;
using RailwayManagementApi.DTOs;
using RailwayManagementApi.DTOs.PassengerDTO;
using RailwayManagementApi.DTOs.TicketDTO;
using RailwayManagementApi.Interfaces;
using RailwayManagementApi.Models;
using Microsoft.AspNetCore.Identity;


namespace RailwayManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly RailwayContext _dbContext;
        private readonly INotificationService _notificationService;


        public TicketController(ITicketService ticketService, RailwayContext context, INotificationService notificationService, UserManager<ApplicationUser> userManager)
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
        [HttpPost("initiate")] //fare calculation and seat avalability
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

        // Response
        // {
        //   "message": "Ticket booked",
        //   "ticketId": 338
        // }
        // [Authorize]
        // [HttpPost("confirm")] //seat no
        // public async Task<IActionResult> ConfirmBookingAsync([FromBody] TicketBookingConfirmDTO booking)
        //     => await _ticketService.ConfirmBookingAsync(booking);

        [Authorize]
        [HttpPost("confirm-payment")]
        public async Task<IActionResult> ConfirmPaymentAndBookTicket([FromBody] PaymentConfirmationDTO paymentDto)
            => await _ticketService.ConfirmPaymentAndBookTicket(paymentDto);


        [Authorize]
        [HttpPost("cancel-passenger")]
        public async Task<IActionResult> CancelPassenger(CancelPassengerDTO dto)
        {
            var result = await _ticketService.CancelPassengerAsync(dto);

            if (result.IsNotFound)
                return NotFound(result.Message);

            if (result.IsError)
                return BadRequest(result.Message + " Details: " + result.ErrorDetails);

            if (result.Success)
                return Ok(new
                {
                    Message = result.Message,
                    Refund = result.RefundAmount,
                    Currency = result.Currency,
                    Note = result.Note
                });

            return BadRequest("Unknown error occurred.");
        }

        [Authorize]
        [HttpGet("details/{ticketId}")]
        public async Task<IActionResult> GetTicketDetails(int ticketId)
        {
            return await _ticketService.GetTicketDetailsAsync(ticketId);
        }

        [Authorize]
        [HttpPost("user-tickets")]
        public async Task<IActionResult> GetUserTickets([FromBody] TicketRequestDto request)
        {
            var tickets = await _ticketService.GetUserTicketsAsync(request);

            if (!tickets.Any()) // If no tickets are found, return 404 Not Found
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(tickets);
        }
    }
}

// Frontend: Initiate → Create-Order → Razorpay Checkout
//            ↓
// Razorpay: Payment Success → Frontend Receives Payment IDs
//            ↓
// Frontend: Calls /api/Ticket/confirm-payment with IDs + Booking Info
//            ↓
// Backend: Validates Signature → Finalizes Ticket Booking
