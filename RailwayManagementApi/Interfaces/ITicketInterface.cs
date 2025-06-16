
using Microsoft.AspNetCore.Mvc;
using RailwayManagementApi.DTOs;
using RailwayManagementApi.DTOs.PassengerDTO;
using RailwayManagementApi.DTOs.TicketDTO;

namespace RailwayManagementApi.Interfaces
{
    public interface ITicketService
    {
        Task<IActionResult> InitiateBookingAsync(TicketBookingRequestDTO booking);
        Task<IActionResult> ConfirmBookingAsync(TicketBookingConfirmDTO booking, PaymentConfirmationDTO paymentDto);
        Task<IActionResult> ConfirmPaymentAndBookTicket(PaymentConfirmationDTO paymentDto);
        // Task<IActionResult> CancelPassengerAsync(CancelPassengerDTO dto);
        Task<CancelPassengerResult> CancelPassengerAsync(CancelPassengerDTO dto);
        Task<IActionResult> GetTicketDetailsAsync(int ticketId);
        
        Task<IEnumerable<object>> GetUserTicketsAsync(TicketRequestDto request);
    }

}