using RailwayManagementApi.DTOs;

public class TrainAvailabilityDayDTO
{
    public DateTime JourneyDate { get; set; }
    public TimeSpan DepartureTime { get; set; }
    public TimeSpan ArrivalTime { get; set; }
    public List<SeatAvailabilityDTO> SeatAvailability { get; set; } = new();
}
