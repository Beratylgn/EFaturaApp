using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFaturaApp.Models
{
    [Table("SHIPMENTS")]
    public class Shipment
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [ValidateNever]
        [Column("SHIPMENTNO")]
        [StringLength(50)]
        public string ShipmentNo { get; set; }

        [Column("SENDERUSERID")]
        public int SenderUserId { get; set; }

        [Required]
        [Column("RECIEVERCUSTOMERID")]
        public int RecieverCustomerId { get; set; }

        [Required]
        [Column("SHIPMENTDATE")]
        public DateTime ShipmentDate { get; set; }

        [Column("CREATEDATE")]
        public DateTime CreateDate { get; set; }

        // Navigation Properties
        [ValidateNever]
        public User SenderUser { get; set; }

        [ValidateNever]
        public Customer RecieverCustomer { get; set; }

        [ValidateNever]
        public ICollection<ShipmentItem> ShipmentItems { get; set; } = new List<ShipmentItem>();
    }
}
