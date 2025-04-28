
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RailwayManagementApi.Models
{
    public class SeatAvailability
    {
        [Key]
        public int AvailabilityID { get; set; }

        [Required]
        public int TrainID { get; set; }

        public DateTime Date { get; set; }

        [Required]
        public int ClassTypeID { get; set; }

        public int RemainingSeats { get; set; }

        [ForeignKey("TrainID")]
        public Train Train { get; set; } = null!;

        [ForeignKey("ClassTypeID")]
        public ClassType ClassType { get; set; } = null!;
    }
}