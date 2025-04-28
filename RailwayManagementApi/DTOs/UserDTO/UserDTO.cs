using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RailwayManagementApi.DTOs.UserDTO
{
    public class UserDTO
    {
        public string Username { get; set; } =null!;
        public string Email { get; set; } = null!;
        public string AadharNumber { get; set; } = null!;
        public List<string> Roles { get; set; } = new();
    }
}