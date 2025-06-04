using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RailwayManagementApi.DTOs;
using RailwayManagementApi.Helper;
using RailwayManagementApi.Interfaces;
using RailwayManagementApi.Models;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtHelper _jwtHelper;
    private readonly INotificationService _notificationService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        JwtHelper jwtHelper,
        INotificationService notificationService)
    {
        _userManager = userManager;
        _jwtHelper = jwtHelper;
        _notificationService = notificationService;
    }

    public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO dto)
    {
        var existingUser = await _userManager.FindByNameAsync(dto.Username);
        if (existingUser != null)
            throw new Exception("Username already exists.");

        var existingEmail = await _userManager.FindByEmailAsync(dto.Email);
        if (existingEmail != null)
            throw new Exception("Email already registered.");

        var user = new ApplicationUser
        {
            UserName = dto.Username,
            Email = dto.Email,
            AadharNumber = dto.AadharNumber
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            var errorMessage = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new Exception(errorMessage);
        }

        // Add default role
        await _userManager.AddToRoleAsync(user, "User");

        var token = await _jwtHelper.GenerateJwtToken(user);

        return new AuthResponseDTO
        {
            Token = token,
            Email = user.Email,
            Username = user.UserName,
            Role = "User"
        };
    }

    public async Task<AuthResponseDTO> LoginAsync(LoginDTO dto)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == dto.Username);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid username or password.");

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!isPasswordValid)
            throw new UnauthorizedAccessException("Invalid username or password.");

        var roles = await _userManager.GetRolesAsync(user);
        var token = await _jwtHelper.GenerateJwtToken(user);

        return new AuthResponseDTO
        {
            Token = token,
            Email = user.Email,
            Username = user.UserName,
            Role = roles.FirstOrDefault() ?? "User"
        };
    }

    public async Task<bool> SendForgotPasswordEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return false;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebUtility.UrlEncode(token);
        await _notificationService.SendResetEmailAsync(user.Email, encodedToken);
        return true;
    }

    public async Task<(bool Succeeded, IEnumerable<string> Errors)> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return (false, new[] { "User not found." });

        var decodedToken = WebUtility.UrlDecode(token);
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, newPassword);

        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }
}
