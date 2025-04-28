using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayManagementApi.Models;
using RailwayManagementApi.Services;
using System.Security.Claims;

namespace RailwayManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/users
        [Authorize(Roles ="Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // GET: api/users/me
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetMe()
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var user = await _userService.GetMeAsync(username);
            return user == null ? NotFound() : Ok(user);
        }

        // GET: api/users/{username}
        [Authorize(Roles ="Admin")]
        [HttpGet("{username}")]
        public async Task<IActionResult> GetUserById(string username)
        {
            var user = await _userService.GetUserByIdAsync(username);
            return user == null ? NotFound() : Ok(user);
        }

        // PUT: api/users/{username}
        [Authorize(Roles ="Admin")]
        [HttpPut("{username}")]
        public async Task<IActionResult> UpdateUser(string username, [FromBody] ApplicationUser updatedUser)
        {
            var result = await _userService.UpdateUserAsync(username, updatedUser);
            return result == null ? NotFound() : Ok(result);
        }

        // DELETE: api/users/{username}
        [Authorize(Roles ="Admin")]
        [HttpDelete("{username}")]
        public async Task<IActionResult> DeleteUser(string username)
        {
            var deleted = await _userService.DeleteUserAsync(username);
            return deleted ? NoContent() : NotFound();
        }
    }
}
