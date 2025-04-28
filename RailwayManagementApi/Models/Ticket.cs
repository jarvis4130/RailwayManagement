using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RailwayManagementApi.Models;

public class Ticket
{
    [Key]
    public int TicketID { get; set; }

    // FK to ApplicationUser (IdentityUser)
    [Required]
    public string UserId { get; set; } = null!;  // Changed Username to UserId

    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; } = null!;  // Set relationship to ApplicationUser via UserId

    [Required]
    public int TrainID { get; set; }

    [ForeignKey("TrainID")]
    public Train Train { get; set; } = null!;

    [Required]
    public int SourceID { get; set; }

    [ForeignKey("SourceID")]
    public Station SourceStation { get; set; } = null!;

    [Required]
    public int DestinationID { get; set; }

    [ForeignKey("DestinationID")]
    public Station DestinationStation { get; set; } = null!;

    [Required]
    public DateTime BookingDate { get; set; }

    [Required]
    public DateTime JourneyDate { get; set; }

    [Required]
    public int ClassTypeID { get; set; }

    [ForeignKey("ClassTypeID")]
    public ClassType ClassType { get; set; } = null!;

    [Required]
    public decimal Fare { get; set; }

    [Required]
    public string Status { get; set; } = null!; // "Booked", "Waiting", etc.

    public bool HasInsurance { get; set; }

    public ICollection<Passenger> Passengers { get; set; } = null!;
    public Payment Payment { get; set; } = null!;
    public ICollection<WaitingList> WaitingLists { get; set; } = null!;
}
