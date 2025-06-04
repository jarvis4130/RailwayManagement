
namespace RailwayManagementApi.DTOs
{
    public class CancelPassengerResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public decimal RefundAmount { get; set; }
        public string Currency { get; set; }
        public string Note { get; set; }
        public bool IsNotFound { get; set; }
        public bool IsError { get; set; }
        public string ErrorDetails { get; set; }
    }
}