namespace RailwayManagementApi.DTOs
{
    public class AddTrainReq
    {
        public string TrainName { get; set; } = null!;
        public string TrainType { get; set; } = null!;
        public int TotalSeats { get; set; }
    }
}