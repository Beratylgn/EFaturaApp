using EFaturaApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace EFaturaApp.Models.ViewModels
{
    public class InvoiceEditViewModel
    {
        public int Id { get; set; }
        public string InvoiceNo { get; set; } = "";
        public DateTime? InvoiceDate { get; set; }
        public string Currency { get; set; } = "TRY";
        public int? ReceiverCustomerId { get; set; }
        public int? ShipmentId { get; set; }

        public List<SelectListItem>? Customers { get; set; }
        public List<SelectListItem>? Products { get; set; }
        public IEnumerable<SelectListItem>? Currencies { get; set; }
        public List<SelectListItem>? Shipments { get; set; }

        public List<InvoiceItemRow> Items { get; set; } = new();

        public class InvoiceItemRow
        {
            public int? ItemId { get; set; }      // düzenleme için şart
            public int ProductId { get; set; }
            public int Quantity { get; set; } = 1;
            public decimal UnitPrice { get; set; }
            public int TaxRate { get; set; } = 18;
        }
        public Invoice Invoice { get; set; }
    }
}
