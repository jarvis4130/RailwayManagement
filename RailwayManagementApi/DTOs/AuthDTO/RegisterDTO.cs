
using System.ComponentModel.DataAnnotations;

namespace RailwayManagementApi.DTOs
{
    public class RegisterDTO
    {
        [Required]
        public string Username { get; set; } = null!;

        [Required, EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; } = null!;

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = null!;

        [Required]
        [RegularExpression(@"^\d{12}$", ErrorMessage = "Aadhar number must be 12 digits")]
        public string AadharNumber { get; set; } = null!;
    }
}