using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RailwayManagementApi.DTOs
{
    public class PassengerInfoDTO
    {
        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Gender { get; set; } = null!;

        [Required]
        public int Age { get; set; }
    }
}