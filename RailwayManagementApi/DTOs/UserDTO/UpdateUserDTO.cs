using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RailwayManagementApi.DTOs.UserDTO
{
    public class UpdateUserDTO
    {
        public string Email { get; set; } = string.Empty;
        public string AadharNumber { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
    }
}