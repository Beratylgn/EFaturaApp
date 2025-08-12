using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFaturaApp.Models
{
    [Table("PRODUCT")]
    public class Product
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("NAME")]
        public string? Name { get; set; }

        [Column("UNIT")]
        public string? Unit { get; set; }

        [Column("UNITPRICE")]
        public decimal? UnitPrice { get; set; }

        [Column("CREATEDATE")]
        public DateTime? CreateDate { get; set; }

        [Column("CURRENCY")]
        public string? Currency { get; set; }

        [Column("UNITQUANTITY")]
        public int? UnitQuantity { get; set; }

        [Column("STOCKCODE")]
        [Required]
        public string StockCode { get; set; }


    }
}
