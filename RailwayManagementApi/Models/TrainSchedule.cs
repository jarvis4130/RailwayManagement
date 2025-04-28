using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RailwayManagementApi.Models
{
    public class TrainSchedule
    {
        [Key]
        public int ScheduleID { get; set; }

        [Required]
        public int TrainID { get; set; }

        [Required]
        public int StationID { get; set; }

        public DateTime ArrivalTime { get; set; }
        public DateTime DepartureTime { get; set; }
        public int SequenceOrder { get; set; }
        public float Fair { get; set; }
        public int DistanceFromSource { get; set; }

        [ForeignKey("TrainID")]
        public Train Train { get; set; } = null!;

        [ForeignKey("StationID")]
        public Station Station { get; set; } = null!;
    }
}