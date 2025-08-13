using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EFaturaApp.Data;
using EFaturaApp.Models;
using EFaturaApp.Models.ViewModels;

namespace EFaturaApp.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly EFaturaContext _ctx;
        public InvoiceController(EFaturaContext ctx) => _ctx = ctx;

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
            vm.Customers = _ctx.Customers
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.CompanyName })
                .ToList();

            vm.Products = _ctx.Products
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToList();

            vm.Currencies = new[] {
                new SelectListItem("TRY","TRY"),
                new SelectListItem("USD","USD"),
                new SelectListItem("EUR","EUR")
            };

            vm.Shipments = _ctx.Shipments
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.ShipmentNo })
                .ToList();
        }

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
            if (!ModelState.IsValid)
            {
                PopulateDropdowns(vm);
                return View(vm);
            }

            using var tx = _ctx.Database.BeginTransaction();

            var invoice = new Invoice
            {
                InvoiceNo = string.IsNullOrWhiteSpace(vm.InvoiceNo) ? GenerateInvoiceNo() : vm.InvoiceNo,
                ReceiverCustomerId = vm.ReceiverCustomerId,
                ShipmentId = vm.ShipmentId,
                IssuerUserId = null,
                InvoiceDate = vm.InvoiceDate,
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

        [HttpGet]
        public IActionResult GetProductInfo(int id)
        {
            var p = _ctx.Products.FirstOrDefault(x => x.Id == id);
            if (p == null) return NotFound();
            return Json(new
            {
                unitPrice = p.UnitPrice ?? 0m,
                currency = p.Currency ?? "TRY",
                unit = p.Unit ?? "Adet"
            });
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

        public IActionResult Details(int id)
        {
            var inv = _ctx.Invoices
                .Include(i => i.ReceiverCustomer)
                .Include(i => i.Items).ThenInclude(it => it.Product)
                .FirstOrDefault(i => i.Id == id);
            if (inv == null) return NotFound();
            return View(inv);
        }

        public IActionResult Index()
        {
            var list = _ctx.Invoices
                .Include(i => i.ReceiverCustomer)
                .OrderByDescending(i => i.Id)
                .ToList();
            return View(list);
        }
    }
}
