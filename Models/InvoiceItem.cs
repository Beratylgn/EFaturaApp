using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFaturaApp.Models
{
    [Table("INVOICESITEMS")]
    public class InvoiceItem
    {
        [Key]
        public int ID { get; set; }

        public int INVOICEID { get; set; }

        public int PRODUCTID { get; set; }

        public int QUANTITY { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UNITPRICE { get; set; }

        public int TAXRATE { get; set; }

        // Navigation properties
        [ValidateNever]
        public Invoice? Invoice { get; set; }

        [NotMapped] // EF bu property üzerinden tekrar ProductId üretmesin diye
        public Product? Product { get; set; }
    }
}