using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFaturaApp.Models
{
    [Table("INVOICES")]
    public class Invoice
    {
        [Key, Column("ID")]
        public int Id { get; set; }

        [Column("INVOICENO"), StringLength(50)]
        public string? InvoiceNo { get; set; }

        [Column("SHIPMENTID")]
        public int? ShipmentId { get; set; }

        [Column("ISSUERUSERID")]
        public int? IssuerUserId { get; set; }

        [Column("RECEIVERCUSTOMERID")]
        public int ReceiverCustomerId { get; set; }

        [Column("INVOICEDATE")]
        public DateTime? InvoiceDate { get; set; }

        [Column("TOTALAMOUNT", TypeName = "decimal(18,2)")]
        public decimal? TotalAmount { get; set; }

        [Column("CREATEDATE")]
        public DateTime? CreateDate { get; set; }

        [Column("CURRENCY")]
        public string? Currency { get; set; }


        // Navigation
        public Customer? ReceiverCustomer { get; set; }
        public List<InvoiceItem> Items { get; set; } = new();
    }
}
