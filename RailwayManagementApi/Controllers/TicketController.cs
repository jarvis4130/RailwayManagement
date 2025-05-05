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
        private readonly UserManager<ApplicationUser> _userManager;

        public TicketController(ITicketService ticketService, RailwayContext context, INotificationService notificationService, UserManager<ApplicationUser> userManager)
        {
            _ticketService = ticketService;
            _dbContext = context;
            _notificationService = notificationService;
            _userManager=userManager;
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
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
                return NotFound("User not found");

            var userId = user.Id;

            var tickets = await _dbContext.Tickets
                .Include(t => t.Passengers)
                .Include(t => t.Train)
                .Include(t => t.SourceStation)
                .Include(t => t.DestinationStation)
                .Include(t => t.ClassType)
                .Where(t => t.UserId == userId)
                .ToListAsync();

            var schedules = await _dbContext.TrainSchedules.ToListAsync();

            var response = tickets.Select(t =>
            {
                var departure = schedules.FirstOrDefault(s => s.TrainID == t.TrainID && s.StationID == t.SourceID);
                var arrival = schedules.FirstOrDefault(s => s.TrainID == t.TrainID && s.StationID == t.DestinationID);

                var departureTime = departure?.DepartureTime ?? DateTime.MinValue;
                var arrivalTime = arrival?.ArrivalTime ?? DateTime.MinValue;
                var durationMinutes = (int)(arrivalTime - departureTime).TotalMinutes;

                return new
                {
                    t.TicketID,
                    t.JourneyDate,
                    t.BookingDate,
                    t.Status,
                    t.Fare,
                    Class = t.ClassType.ClassName,
                    Source = t.SourceStation.StationName,
                    Destination = t.DestinationStation.StationName,
                    DepartureTime = departureTime.ToString("HH:mm"),
                    ArrivalTime = arrivalTime.ToString("HH:mm"),
                    DurationMinutes = durationMinutes,
                    Passengers = t.Passengers.Select(p => new
                    {
                        p.PassengerID,
                        Info = $"{p.Name} ({p.Age}, {p.Gender})"
                    }).ToList()
                };
            });

            return Ok(response);
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
