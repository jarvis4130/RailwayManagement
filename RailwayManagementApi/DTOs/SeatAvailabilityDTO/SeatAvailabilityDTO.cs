

using System.ComponentModel.DataAnnotations;

namespace RailwayManagementApi.DTOs
{
    public class SeatAvailabilityDTO
    {
        [Required]
        public string ClassType { get; set; } = null!;

        [Required]
        public int RemainingSeats { get; set; }

        [Required]
        public int WaitingListCount { get; set; }
    }
}