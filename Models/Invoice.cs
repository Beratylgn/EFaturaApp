using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFaturaApp.Models
{
    [Table("INVOICES")]
    public class Invoice
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }

        [Column("INVOICENO")]
        public string? INVOICENO { get; set; } 

        [Column("SHIPMENTID")]
        public int? SHIPMENTID { get; set; }

        [Column("ISSUERUSERID")]
        public int? ISSUERUSERID { get; set; }

        [Column("RECEIVERCUSTOMERID")]
        public int RECEIVERCUSTOMERID { get; set; }

        [Column("INVOICEDATE")]
        public DateTime? INVOICEDATE { get; set; }

        [Column("TOTALAMOUNT")]
        public decimal? TOTALAMOUNT { get; set; }

        [Column("CREATEDATE")]
        public DateTime? CREATEDATE { get; set; }

        // Navigation
        [ValidateNever]
        public  Customer ReceiverCustomer { get; set; } 

        [ValidateNever]
        public ICollection<InvoiceItem> InvoiceItems { get; set; }
    }
}
