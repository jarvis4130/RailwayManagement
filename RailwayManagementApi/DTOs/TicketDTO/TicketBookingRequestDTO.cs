using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RailwayManagementApi.DTOs
{
    public class TicketBookingRequestDTO
    {
        [Required]
        public string Username { get; set; } = null!;

        [Required]
        public int TrainID { get; set; }

        [Required]
        public int SourceID { get; set; }

        [Required]
        public int DestinationID { get; set; }

        [Required]
        public DateTime JourneyDate { get; set; }

        public int ClassTypeID { get; set; }

        [Required]
        public List<PassengerInfoDTO> Passengers { get; set; } = new();

        public string PaymentMode { get; set; }=null!;  // "Stripe" or "Razorpay"
        // public decimal PaidAmount { get; set; }

        public bool HasInsurance { get; set; }
    }
}