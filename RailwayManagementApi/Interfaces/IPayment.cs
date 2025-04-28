using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace RailwayManagementApi.Interfaces
{
    public interface IPayment
    {
        IActionResult CreateOrder(decimal amount);
    }
}