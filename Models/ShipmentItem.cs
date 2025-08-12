using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFaturaApp.Models
{
    [Table("SHIPMENTITEMS")]
    public class ShipmentItem
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [Column("SHIPMENTID")]
        public int ShipmentId { get; set; }

        [Required]
        [Column("PRODUCTID")]
        public int ProductId { get; set; }

        [Required]
        [Column("QUANTITY")]
        [Range(1, 9999)]
        public int Quantity { get; set; }

        [Required]
        [Column("UNIT")]
        [StringLength(20)]
        public string Unit { get; set; }

        // Navigation Properties
        [ValidateNever]
        public Shipment Shipment { get; set; }

        [ValidateNever]
        public Product Product { get; set; }
    }
}
