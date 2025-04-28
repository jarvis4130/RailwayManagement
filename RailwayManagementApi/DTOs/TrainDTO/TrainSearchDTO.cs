using System.ComponentModel.DataAnnotations;

namespace RailwayManagementApi.DTOs
{
    public class TrainSearchDTO
    {
        [Required(ErrorMessage = "Source is required")]
        public string Source { get; set; } = null!;

        [Required(ErrorMessage = "Destination is required")]
        public string Destination { get; set; } = null!;
        public DateTime? JourneyDate { get; set; }

        public int? ClassTypeId { get; set; } // 1 2 3 hum daalenge frontend se
    }
}