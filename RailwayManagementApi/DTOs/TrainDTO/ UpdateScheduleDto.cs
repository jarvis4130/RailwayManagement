
namespace RailwayManagementApi.DTOs
{
    public class UpdateScheduleDto
    {
        public int TrainID { get; set; }
        public DateTime Date { get; set; }
        public List<ScheduleUpdateEntry> Schedules { get; set; }
    }
}