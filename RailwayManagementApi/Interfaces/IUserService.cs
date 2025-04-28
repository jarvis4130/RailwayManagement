using RailwayManagementApi.DTOs.UserDTO;
using RailwayManagementApi.Models;

namespace RailwayManagementApi.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
        Task<ApplicationUser?> GetUserByIdAsync(string username);
        Task<ApplicationUser?> UpdateUserAsync(string username, ApplicationUser updatedUser);
        Task<bool> DeleteUserAsync(string username);
        Task<ApplicationUser?> GetMeAsync(string username);
    }
}
