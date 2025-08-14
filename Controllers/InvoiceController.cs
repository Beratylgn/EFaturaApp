using EFaturaApp.Controllers;
using EFaturaApp.Data;
using EFaturaApp.Models;
using EFaturaApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
// PDF istersen: using QuestPDF.Fluent; using QuestPDF.Infrastructure;

namespace EFaturaApp.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly EFaturaContext _ctx;
        public InvoiceController(EFaturaContext ctx) => _ctx = ctx;

        // ---------- Yardımcılar ----------
        private string GenerateInvoiceNo()
        {
            var date = DateTime.Now.ToString("yyyyMMdd");
            var seq = (_ctx.Invoices
                .Count(i => i.CreateDate.HasValue && i.CreateDate.Value.Date == DateTime.Today) + 1)
                .ToString("D4");
            return $"INV-{date}-{seq}";
        }
        private void PopulateDropdowns(InvoiceCreateViewModel vm)
        {
            vm.Customers = _ctx.Customers.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.CompanyName }).ToList();
            vm.Products = _ctx.Products.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name }).ToList();
            vm.Currencies = new[] { new SelectListItem("TRY", "TRY"), new SelectListItem("USD", "USD"), new SelectListItem("EUR", "EUR") };
            vm.Shipments = _ctx.Shipments.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.ShipmentNo }).ToList();
        }

        // ---------- Index / Details ----------
        public IActionResult Index()
        {
            var list = _ctx.Invoices
                .Include(i => i.ReceiverCustomer)
                .OrderByDescending(i => i.Id)
                .ToList();
            return View(list);
        }

        public IActionResult Details(int id)
        {
            var inv = _ctx.Invoices
                .Include(i => i.ReceiverCustomer)
                .Include(i => i.Items).ThenInclude(it => it.Product)
                .FirstOrDefault(i => i.Id == id);
            if (inv == null) return NotFound();
            return View(inv);
        }

        // ---------- Create ----------
        [HttpGet]
        public IActionResult Create()
        {
            var vm = new InvoiceCreateViewModel
            {
                InvoiceNo = GenerateInvoiceNo(),
                InvoiceDate = DateTime.Now
            };
            vm.Items.Add(new InvoiceCreateViewModel.InvoiceItemRow());
            PopulateDropdowns(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(InvoiceCreateViewModel vm)
        {
            if (!ModelState.IsValid) { PopulateDropdowns(vm); return View(vm); }

            using var tx = _ctx.Database.BeginTransaction();

            var invoice = new Invoice
            {
                InvoiceNo = string.IsNullOrWhiteSpace(vm.InvoiceNo) ? GenerateInvoiceNo() : vm.InvoiceNo,
                ReceiverCustomerId = vm.ReceiverCustomerId,
                ShipmentId = vm.ShipmentId,
                IssuerUserId = null,
                InvoiceDate = vm.InvoiceDate,
                Currency = vm.Currency,
                CreateDate = DateTime.Now
            };
            _ctx.Invoices.Add(invoice);
            _ctx.SaveChanges();

            decimal total = 0m;
            foreach (var line in vm.Items)
            {
                var item = new InvoiceItem
                {
                    InvoiceId = invoice.Id,
                    ProductId = line.ProductId,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    TaxRate = line.TaxRate
                };
                _ctx.InvoiceItems.Add(item);

                var net = line.UnitPrice * line.Quantity;
                var tax = net * line.TaxRate / 100m;
                total += net + tax;
            }
            invoice.TotalAmount = total;

            _ctx.SaveChanges();
            tx.Commit();

            return RedirectToAction(nameof(Details), new { id = invoice.Id });
        }

        // ---------- Edit ----------
        [HttpGet]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var invoice = _ctx.Invoices
                .Include(i => i.Items)
                .FirstOrDefault(i => i.Id == id);

            if (invoice == null) return NotFound();

            var vm = new InvoiceEditViewModel
            {
                Id = invoice.Id,
                InvoiceNo = invoice.InvoiceNo,
                InvoiceDate = invoice.InvoiceDate,
                Currency = invoice.Currency,
                ReceiverCustomerId = invoice.ReceiverCustomerId,
                ShipmentId = invoice.ShipmentId,
                Items = invoice.Items.Select(i => new InvoiceEditViewModel.InvoiceItemRow
                {
                    ItemId = i.Id,
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TaxRate = i.TaxRate
                }).ToList(),
                Customers = _ctx.Customers.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.CompanyName }).ToList(),
                Products = _ctx.Products.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name }).ToList(),
                Currencies = new[] {
            new SelectListItem("TRY", "TRY"),
            new SelectListItem("USD", "USD"),
            new SelectListItem("EUR", "EUR")
        },
                Shipments = _ctx.Shipments.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.ShipmentNo }).ToList()
            };

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(InvoiceEditViewModel vm)
        {
            var invoice = _ctx.Invoices
                .Include(i => i.Items)
                .FirstOrDefault(i => i.Id == vm.Id);

            if (invoice == null) return NotFound();

            invoice.ReceiverCustomerId = vm.ReceiverCustomerId.GetValueOrDefault();
            invoice.ShipmentId = vm.ShipmentId.GetValueOrDefault();
            invoice.InvoiceDate = vm.InvoiceDate;
            invoice.Currency = vm.Currency;

            var gelenIdler = vm.Items.Where(x => x.ItemId.HasValue).Select(x => x.ItemId.Value).ToList();

            foreach (var eski in invoice.Items.ToList())
            {
                if (!gelenIdler.Contains(eski.Id))
                    _ctx.InvoiceItems.Remove(eski);
            }

            foreach (var row in vm.Items)
            {
                if (row.ItemId.HasValue)
                {
                    var item = invoice.Items.FirstOrDefault(x => x.Id == row.ItemId.Value);
                    if (item != null)
                    {
                        item.ProductId = row.ProductId;
                        item.Quantity = row.Quantity;
                        item.UnitPrice = row.UnitPrice;
                        item.TaxRate = row.TaxRate;
                    }
                }
                else
                {
                    _ctx.InvoiceItems.Add(new InvoiceItem
                    {
                        InvoiceId = invoice.Id,
                        ProductId = row.ProductId,
                        Quantity = row.Quantity,
                        UnitPrice = row.UnitPrice,
                        TaxRate = row.TaxRate
                    });
                }
            }

            // Fatura toplamını yeniden hesapla
            invoice.TotalAmount = invoice.Items.Sum(x => (x.Quantity * x.UnitPrice) * (1 + x.TaxRate / 100m));

            _ctx.SaveChanges();
            return RedirectToAction("Details", new { id = invoice.Id });
        }


        // ---------- Delete (confirm) ----------
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var inv = _ctx.Invoices
                .Include(x => x.ReceiverCustomer)
                .FirstOrDefault(x => x.Id == id);
            if (inv == null) return NotFound();
            return View(inv); // Views/Invoice/Delete.cshtml
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var inv = _ctx.Invoices
                .Include(x => x.Items)
                .FirstOrDefault(x => x.Id == id);
            if (inv == null) return NotFound();

            _ctx.InvoiceItems.RemoveRange(inv.Items);
            _ctx.Invoices.Remove(inv);
            _ctx.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // ---------- GetProductInfo / GetShipmentItems ----------
        [HttpGet]
        public IActionResult GetProductInfo(int id)
        {
            var p = _ctx.Products.FirstOrDefault(x => x.Id == id);
            if (p == null) return NotFound();
            return Json(new { unitPrice = p.UnitPrice ?? 0m, currency = p.Currency ?? "TRY", unit = p.Unit ?? "Adet" });
        }

        [HttpGet]
        public IActionResult GetShipmentItems(int id)
        {
            var items = _ctx.ShipmentItems
                .Include(si => si.Product)
                .Where(si => si.ShipmentId == id)
                .Select(si => new
                {
                    productId = si.ProductId,
                    productName = si.Product != null ? si.Product.Name : "",
                    quantity = si.Quantity,
                    unit = si.Product != null ? si.Product.Unit : "Adet",
                    unitPrice = si.Product != null && si.Product.UnitPrice.HasValue ? si.Product.UnitPrice.Value : 0m,
                    currency = si.Product != null ? si.Product.Currency : "TRY"
                })
                .ToList();
            return Json(items);
        }

        // ---------- ExportExcel (CSV) ----------
        [HttpGet]
        public IActionResult ExportExcel(int id)
        {
            var inv = _ctx.Invoices
                .Include(i => i.ReceiverCustomer)
                .Include(i => i.Items).ThenInclude(it => it.Product)
                .FirstOrDefault(i => i.Id == id);
            if (inv == null) return NotFound();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Fatura No;Tarih;Müşteri;PB;Genel Toplam");
            sb.AppendLine($"{inv.InvoiceNo};{inv.InvoiceDate:dd.MM.yyyy};{inv.ReceiverCustomer?.CompanyName};{inv.Currency};{inv.TotalAmount:0.00}");
            sb.AppendLine();
            sb.AppendLine("Ürün;Miktar;Birim;Birim Fiyat;KDV%;Tutar");
            foreach (var it in inv.Items)
            {
                var unit = it.Product?.Unit ?? "";
                var net = it.Quantity * it.UnitPrice;
                var tax = net * it.TaxRate / 100m;
                var total = net + tax;
                sb.AppendLine($"{it.Product?.Name};{it.Quantity};{unit};{it.UnitPrice:0.00};{it.TaxRate};{total:0.00}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"Invoice_{inv.InvoiceNo}.csv";
            return File(bytes, "text/csv; charset=utf-8", fileName);
        }

        // ---------- ExportPdf (opsiyonel: QuestPDF) ----------
        [HttpGet]
        public IActionResult ExportPdf(int id)
        {
            // QuestPDF kullanacaksan NuGet ekle ve burada belgeyi oluştur.
            // Şimdilik 200 OK text döndürelim ki route/onay test edilsin.
            return Content($"PDF export here for invoice #{id}");
        }
    }
}
