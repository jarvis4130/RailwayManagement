using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RailwayManagementApi.Models
{
    public class Notification
    {
        [Key]
        public int NotificationID { get; set; }

        [Required]
        public string UserName { get; set; } = null!;

        [Required]
        public int TrainID { get; set; }

        public DateTime NotifiedOn { get; set; }
        public string? Type { get; set; }
        public string? Message { get; set; }

        [ForeignKey("UserName")]
        public ApplicationUser User { get; set; } = null!;

        [ForeignKey("TrainID")]
        public Train Train { get; set; } = null!;
    }
}