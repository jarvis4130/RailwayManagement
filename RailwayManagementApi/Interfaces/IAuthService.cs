using RailwayManagementApi.DTOs;

namespace RailwayManagementApi.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> RegisterAsync(RegisterDTO dto);
        Task<AuthResponseDTO> LoginAsync(LoginDTO dto);
        Task<bool> SendForgotPasswordEmailAsync(string email);
        Task<(bool Succeeded, IEnumerable<string> Errors)> ResetPasswordAsync(string email, string token, string newPassword);
    }
}
