
namespace RailwayManagementApi.DTOs
{
    public class TrainDTOAdmin
    {
        public int TrainID { get; set; }
        public string TrainName { get; set; } = null!;
        public string TrainType { get; set; } = null!;
        public int TotalSeats { get; set; }
        public string? RunningDays { get; set; }
    }
}