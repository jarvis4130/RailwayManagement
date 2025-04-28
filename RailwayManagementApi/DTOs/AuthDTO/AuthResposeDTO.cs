namespace RailwayManagementApi.DTOs
{
    public class AuthResponseDTO
    {
        public string Token { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Role { get; set; }
    }
}
