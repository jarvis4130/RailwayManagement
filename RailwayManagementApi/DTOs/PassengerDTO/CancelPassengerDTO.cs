

namespace RailwayManagementApi.DTOs.PassengerDTO
{
    public class CancelPassengerDTO
    {
        public int PassengerId { get; set; }
        public string UserId { get; set; } = null!;
    }
}