
using System.ComponentModel.DataAnnotations;

namespace RailwayManagementApi.Models
{
    public class Refund
    {
        [Key]
        public int RefundID { get; set; }
        public int PaymentID { get; set; }
        public string RazorpayRefundId { get; set; }
        public decimal Amount { get; set; }
        public DateTime RefundedOn { get; set; }

        public virtual Payment Payment { get; set; }
    }

}