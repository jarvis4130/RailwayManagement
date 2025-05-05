using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RailwayManagementApi.DTOs.PassengerDTO;

namespace RailwayManagementApi.DTOs.TicketDTO
{
    public class TicketDetailsDTO
    {
        public string? Source { get; set; }
        public string? Destination { get; set; }
        public string? BookingDate { get; set; }
        public string? JourneyDate { get; set; }
        public string? Class { get; set; }
        public decimal? Fare { get; set; }
        public string? Status { get; set; }
        public List<PassengerDisplayDTO> PassengerInfo { get; set; }
        public string DepartureTime { get; set; }
        public string ArrivalTime { get; set; }
        public int? DurationMinutes { get; set; }
    }
}