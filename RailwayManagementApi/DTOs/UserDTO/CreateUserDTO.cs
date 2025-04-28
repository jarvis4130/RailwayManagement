using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RailwayManagementApi.DTOs.UserDTO
{
    public class CreateUserDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string AadharNumber { get; set; }
        public string Role { get; set; } = "User";
    }
}