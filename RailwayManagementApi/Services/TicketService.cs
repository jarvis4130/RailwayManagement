
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

namespace RailwayManagementApi.Services
{
    public class TicketService : ITicketService
    {
        private readonly RailwayContext _dbContext;
        private readonly TicketHelper _ticketHelper;
        private readonly IConfiguration _config;
        
    private readonly INotificationService _notificationService;

        public TicketService(INotificationService notificationService,RailwayContext dbContext, TicketHelper ticketHelper, IConfiguration config)
        {
            _dbContext = dbContext;
            _ticketHelper = ticketHelper;
            _config = config;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> InitiateBookingAsync(TicketBookingRequestDTO booking)
        {
            var sourceSchedule = await _dbContext.TrainSchedules
                .FirstOrDefaultAsync(ts => ts.TrainID == booking.TrainID && ts.StationID == booking.SourceID);
            var destinationSchedule = await _dbContext.TrainSchedules
                .FirstOrDefaultAsync(ts => ts.TrainID == booking.TrainID && ts.StationID == booking.DestinationID);

            if (sourceSchedule == null || destinationSchedule == null)
                return new BadRequestObjectResult("Invalid source or destination station for the selected train.");

            var totalStops = await _dbContext.TrainSchedules
                .Where(ts => ts.TrainID == booking.TrainID)
                .MaxAsync(ts => ts.SequenceOrder);

            decimal fare = await _ticketHelper.CalculateFare(booking, sourceSchedule, destinationSchedule, totalStops);

            if (fare == 0)
                return new BadRequestObjectResult("Fare not available for the selected route.");

            var seatAvailability = await _dbContext.SeatAvailabilities
                .FirstOrDefaultAsync(sa => sa.TrainID == booking.TrainID
                                        && sa.ClassTypeID == booking.ClassTypeID
                                        && sa.Date.Date == booking.JourneyDate.Date);

            if (seatAvailability == null)
                return new BadRequestObjectResult("Seat availability not found for given train and class type on selected date.");

            int remainingSeats = seatAvailability.RemainingSeats;
            int totalSeats = _ticketHelper.GetTotalSeatsForClass(booking.TrainID, booking.ClassTypeID);

            return new OkObjectResult(new
            {
                fare,
                classTypeId = booking.ClassTypeID,
                totalSeats,
                availableSeats = remainingSeats,
                passengerCount = booking.Passengers.Count,
                isAvailable = remainingSeats >= booking.Passengers.Count
            });
        }

        public async Task<IActionResult> ConfirmBookingAsync(TicketBookingConfirmDTO booking)
        {
            var seatRecord = await _dbContext.SeatAvailabilities
                .FirstOrDefaultAsync(sa => sa.TrainID == booking.TrainID
                                        && sa.ClassTypeID == booking.ClassTypeID
                                        && sa.Date.Date == booking.JourneyDate.Date);

            if (seatRecord == null)
                return new BadRequestObjectResult("Seat availability not found for the selected train, class, and date.");

            int remainingSeats = seatRecord.RemainingSeats;

            var sourceSchedule = await _dbContext.TrainSchedules.FirstOrDefaultAsync(ts =>
                ts.TrainID == booking.TrainID && ts.StationID == booking.SourceID);
            var destinationSchedule = await _dbContext.TrainSchedules.FirstOrDefaultAsync(ts =>
                ts.TrainID == booking.TrainID && ts.StationID == booking.DestinationID);

            if (sourceSchedule == null || destinationSchedule == null)
                return new BadRequestObjectResult("Invalid source or destination station for the selected train.");

            decimal fare = booking.PaidAmount;

            var ticket = new Ticket
            {
                UserId = booking.UserId,
                TrainID = booking.TrainID,
                SourceID = booking.SourceID,
                DestinationID = booking.DestinationID,
                BookingDate = DateTime.Now,
                JourneyDate = booking.JourneyDate,
                ClassTypeID = booking.ClassTypeID,
                Fare = fare,
                HasInsurance = booking.HasInsurance
            };

            if (remainingSeats >= booking.Passengers.Count)
            {
                ticket.Status = "Booked";
                _dbContext.Tickets.Add(ticket);
                await _dbContext.SaveChangesAsync();

                int totalSeats = _ticketHelper.GetTotalSeatsForClass(booking.TrainID, booking.ClassTypeID);
                int bookedCount = totalSeats - seatRecord.RemainingSeats;

                for (int i = 0; i < booking.Passengers.Count; i++)
                {
                    string seatNumber = _ticketHelper.GenerateSeatNumber(booking.ClassTypeID, booking.TrainID, bookedCount + i);
                    _dbContext.Passengers.Add(new Passenger
                    {
                        TicketID = ticket.TicketID,
                        Name = booking.Passengers[i].Name,
                        Gender = booking.Passengers[i].Gender,
                        Age = booking.Passengers[i].Age,
                        SeatNumber = seatNumber
                    });
                }

                seatRecord.RemainingSeats -= booking.Passengers.Count;
            }
            else
            {
                ticket.Status = "Waiting";
                _dbContext.Tickets.Add(ticket);
                await _dbContext.SaveChangesAsync();

                foreach (var p in booking.Passengers)
                {
                    _dbContext.Passengers.Add(new Passenger
                    {
                        TicketID = ticket.TicketID,
                        Name = p.Name,
                        Gender = p.Gender,
                        Age = p.Age,
                        SeatNumber = "-"
                    });
                }

                var wlPosition = await _dbContext.WaitingLists
                    .Where(w => w.TrainID == booking.TrainID &&
                                w.ClassTypeID == booking.ClassTypeID &&
                                w.RequestDate == booking.JourneyDate)
                    .CountAsync();

                _dbContext.WaitingLists.Add(new WaitingList
                {
                    TicketID = ticket.TicketID,
                    TrainID = booking.TrainID,
                    ClassTypeID = booking.ClassTypeID,
                    RequestDate = booking.JourneyDate,
                    Position = wlPosition + 1
                });
            }

            _dbContext.Payments.Add(new Models.Payment
            {
                TicketID = ticket.TicketID,
                PaymentMode = booking.PaymentMode,
                Amount = fare,
                PaymentDate = DateTime.Now,
                IsRefunded = false
            });

            await _dbContext.SaveChangesAsync();

            if (ticket.Status == "Booked")
                return new OkObjectResult(new { message = "Ticket booked", ticketId = ticket.TicketID });

            var position = await _dbContext.WaitingLists
                .Where(w => w.TicketID == ticket.TicketID)
                .Select(w => w.Position)
                .FirstAsync();

            return new OkObjectResult(new { message = "Added to waiting list", ticketId = ticket.TicketID, position });
        }

        public async Task<IActionResult> ConfirmPaymentAndBookTicket(PaymentConfirmationDTO paymentDto)
        {
            var client = new RazorpayClient(_config["Razorpay:Key"], _config["Razorpay:Secret"]);
            var attributes = new Dictionary<string, string>
        {
            { "razorpay_payment_id", paymentDto.RazorpayPaymentId },
            { "razorpay_order_id", paymentDto.RazorpayOrderId },
            { "razorpay_signature", paymentDto.RazorpaySignature }
        };

            try
            {
                Utils.verifyPaymentSignature(attributes);
            }
            catch
            {
                return new BadRequestObjectResult("Invalid payment signature.");
            }

            return await ConfirmBookingAsync(paymentDto.BookingInfo);
        }


        public async Task<IActionResult> CancelPassengerAsync(CancelPassengerDTO dto)
        {
            var passenger = await _dbContext.Passengers
                .Include(p => p.Ticket)
                .FirstOrDefaultAsync(p => p.PassengerID == dto.PassengerId && p.Ticket.UserId == dto.UserId);

            if (passenger == null)
                return new NotFoundObjectResult("Passenger not found or not owned by this user.");

            var ticket = passenger.Ticket;
            var cancelledSeatNumber = passenger.SeatNumber;

            // Remove passenger
            _dbContext.Passengers.Remove(passenger);

            // Free seat
            var seat = await _dbContext.SeatAvailabilities.FirstOrDefaultAsync(sa =>
                sa.TrainID == ticket.TrainID &&
                sa.ClassTypeID == ticket.ClassTypeID &&
                sa.Date == ticket.JourneyDate);

            if (seat != null)
            {
                seat.RemainingSeats += 1;
                _dbContext.SeatAvailabilities.Update(seat);
            }

            // Promote first passenger from waiting list (if any)
            var waitingListEntries = await _dbContext.WaitingLists
                .Where(w => w.TrainID == ticket.TrainID
                         && w.ClassTypeID == ticket.ClassTypeID
                         && w.RequestDate.Date == ticket.JourneyDate.Date)
                .OrderBy(w => w.Position)
                .ToListAsync();

            if (waitingListEntries.Any())
            {
                var firstInLine = waitingListEntries.First();

                var waitingTicket = await _dbContext.Tickets
                    .FirstOrDefaultAsync(t => t.TicketID == firstInLine.TicketID && t.Status == "Waiting");

                if (waitingTicket != null && waitingTicket.ClassTypeID == ticket.ClassTypeID)
                {
                    waitingTicket.Status = "Booked";
                    _dbContext.Tickets.Update(waitingTicket);

                    // Assign seat number of cancelled passenger
                    var promotedPassenger = await _dbContext.Passengers
                        .FirstOrDefaultAsync(p => p.TicketID == waitingTicket.TicketID);

                    if (promotedPassenger != null)
                    {
                        promotedPassenger.SeatNumber = cancelledSeatNumber;
                        _dbContext.Passengers.Update(promotedPassenger);
                    }

                    // Update seat availability
                    seat.RemainingSeats -= 1;
                    _dbContext.SeatAvailabilities.Update(seat);

                    // Remove from waiting list
                    _dbContext.WaitingLists.Remove(firstInLine);

                    // Reorder remaining positions
                    var remainingWaiting = waitingListEntries.Skip(1).ToList();
                    for (int i = 0; i < remainingWaiting.Count; i++)
                    {
                        remainingWaiting[i].Position = i + 1;
                    }

                    // Send Notification
                    var user = await _dbContext.Users.FindAsync(waitingTicket.UserId);
                    if (user != null)
                    {
                        var subject = "Your Railway Ticket is Confirmed!";
                        var message = $@"
                                 <p>Dear {user.UserName},</p>
                                 <p>Your waiting ticket (ID: <strong>{waitingTicket.TicketID}</strong>) has been <strong>confirmed</strong>.</p>
                                 <p>You have been allotted Seat Number: <strong>{promotedPassenger.SeatNumber}</strong>.</p>
                                 <br />
                                 <p>Thank you for choosing our railway service.</p>
                                 ";
                        // Send Email
                        await _notificationService.SendEmailAsync(user.Email, subject, message);

                        // Create Notification record
                        var notification = new Notification
                        {
                            UserName = user.UserName,
                            TrainID = ticket.TrainID,
                            NotifiedOn = DateTime.Now,
                            Type = "Ticket Confirmed",
                            Message = $"Your waiting ticket (ID: {waitingTicket.TicketID}) has been confirmed. Seat Number: {promotedPassenger.SeatNumber}."
                        };
                        _dbContext.Notifications.Add(notification);
                    }
                }
            }

            // If no passengers remain on this ticket, cancel the ticket
            var remaining = await _dbContext.Passengers
                .Where(p => p.TicketID == ticket.TicketID)
                .ToListAsync();

            if (!remaining.Any())
            {
                ticket.Status = "Cancelled";
                _dbContext.Tickets.Update(ticket);
            }

            // Refund logic (50% of this passenger’s fare)
            decimal perPassengerFare = ticket.Fare / (remaining.Count + 1);
            decimal refund = perPassengerFare * 0.5m;

            await _dbContext.SaveChangesAsync();

            return new OkObjectResult(new
            {
                Message = "Passenger cancelled.",
                Refund = refund,
                Currency = "INR",
                Note = "50% refund as per cancellation policy."
            });
        }

    }

}