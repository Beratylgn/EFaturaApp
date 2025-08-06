using EFaturaApp.Models;
using System.ComponentModel.DataAnnotations;

namespace EFaturaApp.Models
{
    public class InvoiceItem
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int INVOICEID { get; set; }

        [Required]
        public int PRODUCTID { get; set; }

        [Required]
        public int QUANTITY { get; set; }

        [Required]
        public decimal UNITPRICE { get; set; }

        [Required]
        public int TAXRATE { get; set; }

        // Navigation Properties
        public Invoice Invoice { get; set; }
        public Product Product { get; set; }
    }
}
