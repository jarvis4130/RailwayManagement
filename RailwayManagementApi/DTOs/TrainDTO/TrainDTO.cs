using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RailwayManagementApi.DTOs
{
    public class TrainDTO
    {
        public int TrainId { get; set; }

        [Required]
        public string TrainName { get; set; } = null!;

        [Required]
        public string Source { get; set; } = null!;

        [Required]
        public string Destination { get; set; } = null!;

        public TimeSpan DepartureTime { get; set; }

        public TimeSpan ArrivalTime { get; set; }

        public int DurationMinutes { get; set; }

        public DateTime? JourneyDate { get; set; }  // Optional: null means return trains for next 7 days

        public List<SeatAvailabilityDTO> SeatAvailability { get; set; }=null!;

    }

}