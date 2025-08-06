using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFaturaApp.Models
{
    [Table("CUSTOMER")]
    public class Customer
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("COMPANYNAME")]
        public string CompanyName { get; set; }

        [Column("TAXNUMBER")]
        public string TaxNumber { get; set; }

        [Column("ADDRESS")]
        public string Address { get; set; }

        [Column("EMAIL")]
        public string Email { get; set; }

        [Column("CREATEDAT")]
        public DateTime CreatedAt { get; set; }

        public ICollection<Invoice> Invoices { get; set; }
        public int ID => Id;

        [NotMapped]
        public string Name => CompanyName;
    }
}
