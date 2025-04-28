

using System.ComponentModel.DataAnnotations;

namespace RailwayManagementApi.Models
{
    public class ClassType
    {
        [Key]
        public int ClassTypeID { get; set; }

        [Required]
        public string ClassName { get; set; } = null!;

        public ICollection<SeatAvailability> SeatAvailabilities { get; set; } = new List<SeatAvailability>();
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public ICollection<WaitingList> WaitingLists { get; set; } = new List<WaitingList>();
    }
}