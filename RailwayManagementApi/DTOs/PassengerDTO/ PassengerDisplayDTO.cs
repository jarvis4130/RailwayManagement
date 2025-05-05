using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RailwayManagementApi.DTOs.PassengerDTO
{
    public class PassengerDisplayDTO
    {
        public int PassengerID { get; set; }
        public string? Name { get; set; }
        public string? Seat { get; set; }
    }
}