using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace RailwayManagementApi.Models
{
    public class ApplicationUser: IdentityUser
    {


        [Required]
        public string AadharNumber { get; set; } = null!;

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}