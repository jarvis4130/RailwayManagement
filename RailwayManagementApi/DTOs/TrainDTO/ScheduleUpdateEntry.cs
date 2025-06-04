
namespace RailwayManagementApi.DTOs
{
    public class ScheduleUpdateEntry
    {
        public int StationID { get; set; }
        public DateTime ArrivalTime { get; set; }
        public DateTime DepartureTime { get; set; }
        public int SequenceOrder { get; set; }
        public int Fare { get; set; }
        public int DistanceFromSource { get; set; }
    }
}