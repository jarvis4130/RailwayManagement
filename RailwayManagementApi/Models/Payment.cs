using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RailwayManagementApi.Models
{
    public class Payment
    {
        [Key]
        public int PaymentID { get; set; }

        [Required]
        public int TicketID { get; set; }

        public decimal Amount { get; set; }

        [Required]
        public string RazorpayPaymentId { get; set; } = null!;
    
        public bool IsRefunded {get;set;}

        [ForeignKey("TicketID")]
        public Ticket Ticket { get; set; } = null!;


        public DateTime PaymentDate { get; set; }
        public string? PaymentMode { get; set; }
        public string? Status { get; set; }
        public bool IncludesInsurance { get; set; }
        public decimal InsuranceAmount { get; set; }

    }
}