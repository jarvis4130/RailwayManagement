using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using RailwayManagementApi.DTOs.TicketDTO;

namespace RailwayManagementApi.DTOs
{
    // sdjf
    public class PaymentConfirmationDTO
    {
        [Required]
        public string RazorpayPaymentId { get; set; }=null!;
        [Required]
        public string RazorpayOrderId { get; set; }=null!;
        [Required]
        public string RazorpaySignature { get; set; }=null!;
        [Required]
        public TicketBookingConfirmDTO BookingInfo { get; set; }=null!;
    }

}