using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RailwayManagementApi.DTOs.UserDTO;
using RailwayManagementApi.Interfaces;
using RailwayManagementApi.Models;

namespace RailwayManagementApi.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var userDtoList = new List<UserDTO>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtoList.Add(new UserDTO
                {
                    Username = user.UserName!,
                    Email = user.Email!,
                    AadharNumber = user.AadharNumber,
                    Roles = roles.ToList()
                });
            }

            return userDtoList;
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string username)
        {
            return await _userManager.FindByNameAsync(username);
        }

        public async Task<ApplicationUser?> UpdateUserAsync(string username, ApplicationUser updatedUser)
        {
            var existingUser = await _userManager.FindByNameAsync(username);
            if (existingUser == null) return null;

            existingUser.Email = updatedUser.Email;
            existingUser.PhoneNumber = updatedUser.PhoneNumber;
            existingUser.AadharNumber = updatedUser.AadharNumber;

            var result = await _userManager.UpdateAsync(existingUser);
            return result.Succeeded ? existingUser : null;
        }

        public async Task<bool> DeleteUserAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public async Task<ApplicationUser?> GetMeAsync(string username)
        {
            return await _userManager.FindByNameAsync(username);
        }
    }
}
