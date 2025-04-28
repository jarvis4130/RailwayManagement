using System.ComponentModel.DataAnnotations;

namespace RailwayManagementApi.Models
{
    public class Station
    {
        [Key]
        public int StationID { get; set; }

        [Required]
        public string StationName { get; set; } = null!;

        [Required]
        public string Location { get; set; }=null!;

        public ICollection<TrainSchedule> TrainSchedules { get; set; } = new List<TrainSchedule>();
    }
}