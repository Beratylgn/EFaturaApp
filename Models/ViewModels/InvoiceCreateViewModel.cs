using EFaturaApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace EFaturaApp.Models.ViewModels
{
    public class InvoiceCreateViewModel
    {
        // ----- Header -----
        public string? InvoiceNo { get; set; }

        [Required]
        public int ReceiverCustomerId { get; set; }

        public int? ShipmentId { get; set; }

        public DateTime InvoiceDate { get; set; } = DateTime.Now;

        [Required]
        public string Currency { get; set; } = "TRY"; // Fatura para birimi

        // ----- Lines -----
        public List<InvoiceItemRow> Items { get; set; } = new();

        public class InvoiceItemRow
        {
            [Required]
            public int ProductId { get; set; }

            [Range(1, int.MaxValue)]
            public int Quantity { get; set; } = 1;

            [Range(0, double.MaxValue)]
            public decimal UnitPrice { get; set; } // Fatura PB

            public int TaxRate { get; set; } = 18; // % KDV (varsayılan 18)

            // UI-only (DB'ye gitmez)
            public decimal? OriginalUnitPrice { get; set; } // Ürünün kendi PB birim fiyatı
            public string? OriginalCurrency { get; set; }
            public string? UnitLabel { get; set; } // Adet/Metre vb. yalnızca görüntü
        }

        // ----- Dropdown kaynakları -----
        public IEnumerable<SelectListItem>? Customers { get; set; }
        public IEnumerable<SelectListItem>? Products { get; set; }
        public IEnumerable<SelectListItem>? Currencies { get; set; }
        public IEnumerable<SelectListItem>? Shipments { get; set; }
    }
}
