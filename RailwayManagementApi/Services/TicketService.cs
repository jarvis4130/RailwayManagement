
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

        public TicketService(INotificationService notificationService, RailwayContext dbContext, TicketHelper ticketHelper, IConfiguration config)
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
                fare = fare * booking.Passengers.Count,
                classTypeId = booking.ClassTypeID,
                totalSeats,
                availableSeats = remainingSeats,
                passengerCount = booking.Passengers.Count,
                isAvailable = remainingSeats >= booking.Passengers.Count
            });
        }

        public async Task<IActionResult> ConfirmBookingAsync(TicketBookingConfirmDTO booking, PaymentConfirmationDTO paymentDto)
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
                RazorpayPaymentId=paymentDto.RazorpayPaymentId, 
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

            return await ConfirmBookingAsync(paymentDto.BookingInfo,paymentDto);
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
            await _dbContext.SaveChangesAsync();

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
                Console.WriteLine(ticket);
                _dbContext.Tickets.Update(ticket);
                await _dbContext.SaveChangesAsync();
            }

            // Refund logic (50% of this passenger’s fare)
            decimal perPassengerFare = ticket.Fare / (remaining.Count + 1);
            decimal refund = perPassengerFare * 0.5m;

            await _dbContext.SaveChangesAsync();

            // refund logic
            string key = _config["Razorpay:Key"];
            string secret = _config["Razorpay:Secret"];


            var payment = await _dbContext.Payments.FirstOrDefaultAsync(p => p.TicketID == ticket.TicketID);

            if (payment != null && !string.IsNullOrEmpty(payment.RazorpayPaymentId) && !payment.IsRefunded)
            {
                try
                {
                    var razorpayClient = new RazorpayClient(key, secret);

                    int refundAmountInPaise = (int)(refund * 100); // Razorpay expects paise

                    var paymentObj = razorpayClient.Payment.Fetch(payment.RazorpayPaymentId);

                    Dictionary<string, object> refundParams = new Dictionary<string, object>
        {
            { "amount", refundAmountInPaise },
            { "notes", new Dictionary<string, string> { { "reason", "Passenger cancellation - 50% refund" } } }
        };

                    var refundResponse = paymentObj.Refund(refundParams);

                    // Mark refund status
                    payment.IsRefunded = true;
                    _dbContext.Payments.Update(payment);
                    await _dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    return new BadRequestObjectResult("Razorpay refund failed: " + ex.Message);
                }
            }
            else
            {
                return new NotFoundObjectResult("Payment record not found or already refunded.");
            }

            return new OkObjectResult(new
            {
                Message = "Passenger cancelled.",
                Refund = refund,
                Currency = "INR",
                Note = "50% refund as per cancellation policy."
            });
        }


        // public async Task<IActionResult> GetTicketDetailsAsync(int ticketId)
        // {
        //     var ticket = await _dbContext.Tickets
        //         .Include(t => t.SourceStation)
        //         .Include(t => t.DestinationStation)
        //         .Include(t => t.ClassType)
        //         .Include(t => t.Passengers)  // Include Passengers
        //         .Include(t => t.WaitingLists) // Include WaitingLists
        //         .FirstOrDefaultAsync(t => t.TicketID == ticketId);

        //     if (ticket == null)
        //         return new NotFoundObjectResult(new { message = "Ticket not found" });

        //     var passengerInfo = new List<string>();

        //     if (ticket.Status == "Booked")
        //     {
        //         passengerInfo = ticket.Passengers
        //             .Select(p => $"{p.Name} - {p.SeatNumber}")
        //             .ToList();
        //     }
        //     else if (ticket.Status == "Waiting")
        //     {
        //         passengerInfo = ticket.WaitingLists
        //             .Select(w =>
        //             {
        //                 // Get the corresponding passenger for this ticket's WaitingList entry
        //                 var passenger = ticket.Passengers.FirstOrDefault(p => p.TicketID == ticketId);
        //                 return passenger != null ? $"{passenger.Name} - Waiting No: {w.Position}" : $"Waiting No: {w.Position}";
        //             })
        //             .ToList();
        //     }

        //     // Create DTO for ticket details
        //     var ticketDetailsDto = new TicketDetailsDTO
        //     {
        //         Source = ticket.SourceStation.StationName,
        //         Destination = ticket.DestinationStation.StationName,
        //         BookingDate = ticket.BookingDate.ToString("yyyy-MM-dd"),
        //         JourneyDate = ticket.JourneyDate.ToString("yyyy-MM-dd"),
        //         Class = ticket.ClassType.ClassName,
        //         Fare = ticket.Fare,
        //         Status = ticket.Status,
        //         PassengerInfo = passengerInfo // List now contains both booked passengers and waiting passengers with positions
        //     };

        //     return new OkObjectResult(ticketDetailsDto);
        // }
        public async Task<IActionResult> GetTicketDetailsAsync(int ticketId)
        {
            var ticket = await _dbContext.Tickets
                .Include(t => t.SourceStation)
                .Include(t => t.DestinationStation)
                .Include(t => t.ClassType)
                .Include(t => t.Passengers)
                .Include(t => t.WaitingLists)
                .Include(t => t.Train)
                .FirstOrDefaultAsync(t => t.TicketID == ticketId);

            if (ticket == null)
                return new NotFoundObjectResult(new { message = "Ticket not found" });

            // Fetch source & destination schedules
            var sourceSchedule = await _dbContext.TrainSchedules
                .FirstOrDefaultAsync(s => s.TrainID == ticket.TrainID && s.StationID == ticket.SourceID);

            var destinationSchedule = await _dbContext.TrainSchedules
                .FirstOrDefaultAsync(s => s.TrainID == ticket.TrainID && s.StationID == ticket.DestinationID);

            string? departureTime = sourceSchedule?.DepartureTime.ToString("HH:mm");
            string? arrivalTime = destinationSchedule?.ArrivalTime.ToString("HH:mm");
            int? durationMinutes = null;

            if (sourceSchedule != null && destinationSchedule != null)
            {
                durationMinutes = (int)(destinationSchedule.ArrivalTime - sourceSchedule.DepartureTime).TotalMinutes;
            }

            List<PassengerDisplayDTO> passengerInfo = new();

            if (ticket.Status == "Booked")
            {
                passengerInfo = ticket.Passengers
                    .Select(p => new PassengerDisplayDTO
                    {
                        PassengerID = p.PassengerID,
                        Name = p.Name,
                        Seat = p.SeatNumber
                    }).ToList();
            }
            else if (ticket.Status == "Waiting")
            {
                passengerInfo = ticket.WaitingLists
                    .Select(w =>
                    {
                        var passenger = ticket.Passengers.FirstOrDefault(p => p.PassengerID == w.PassengerID);
                        return new PassengerDisplayDTO
                        {
                            PassengerID = passenger?.PassengerID ?? 0,
                            Name = passenger?.Name ?? "Unknown",
                            Seat = $"Waiting No: {w.Position}"
                        };
                    }).ToList();
            }

            var ticketDetailsDto = new TicketDetailsDTO
            {
                Source = ticket.SourceStation.StationName,
                Destination = ticket.DestinationStation.StationName,
                BookingDate = ticket.BookingDate.ToString("yyyy-MM-dd"),
                JourneyDate = ticket.JourneyDate.ToString("yyyy-MM-dd"),
                Class = ticket.ClassType.ClassName,
                Fare = ticket.Fare,
                Status = ticket.Status,
                PassengerInfo = passengerInfo,
                DepartureTime = departureTime,
                ArrivalTime = arrivalTime,
                DurationMinutes = durationMinutes
            };

            return new OkObjectResult(ticketDetailsDto);
        }

    }

}