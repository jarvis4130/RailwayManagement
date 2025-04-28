using System.ComponentModel.DataAnnotations;

namespace RailwayManagementApi.Models
{
    public class Train
    {
        [Key]
        public int TrainID { get; set; }

        [Required]
        public string TrainName { get; set; } = null!;

        [Required]
        public string TrainType { get; set; }=null!;

        [Required]
        public int TotalSeats { get; set; }

        public string? RunningDays { get; set; }

        public ICollection<TrainSchedule> TrainSchedules { get; set; } = new List<TrainSchedule>();
        public ICollection<SeatAvailability> SeatAvailabilities { get; set; } = new List<SeatAvailability>();
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}