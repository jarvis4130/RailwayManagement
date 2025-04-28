
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RailwayManagementApi.Models
{
    public class Passenger
    {
        [Key]
        public int PassengerID { get; set; }

        [Required]
        public int TicketID { get; set; }

        public string Name { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public int Age { get; set; }
        public string? SeatNumber { get; set; }

        [ForeignKey("TicketID")]
        public Ticket Ticket { get; set; } = null!;
    }
}