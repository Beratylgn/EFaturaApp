using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFaturaApp.Models
{
    [Table("INVOICESITEMS")]
    public class InvoiceItem
    {
        [Key, Column("ID")]
        public int Id { get; set; }

        [Column("INVOICEID")]
        public int InvoiceId { get; set; }

        [Column("PRODUCTID")]
        public int ProductId { get; set; }

        [Column("QUANTITY")]
        public int Quantity { get; set; }

        [Column("UNITPRICE", TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }   // Faturadaki para birimindeki birim fiyat

        [Column("TAXRATE")]
        public int TaxRate { get; set; }         // % KDV

        // Navigation
        public Product? Product { get; set; }
        public Invoice? Invoice { get; set; }
    }
}
