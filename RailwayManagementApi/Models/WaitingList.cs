

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RailwayManagementApi.Models
{
    public class WaitingList
    {
        [Key]
        public int WaitingListID { get; set; }

        [Required]
        public int TicketID { get; set; }

        [Required]
        public int TrainID { get; set; }

        [Required]
        public int ClassTypeID { get; set; }

        public DateTime RequestDate { get; set; }
        public int Position { get; set; }

        [ForeignKey("TicketID")]
        public Ticket Ticket { get; set; }=null!;

        [ForeignKey("TrainID")]
        public Train Train { get; set; }=null!;
        
        [ForeignKey("ClassTypeID")]
        public ClassType ClassType { get; set; }=null!;
    }
}