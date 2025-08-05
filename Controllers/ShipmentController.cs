using EFaturaApp.Data;
using EFaturaApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;
using ClosedXML.Excel;
using System.IO;

namespace EFaturaApp.Controllers
{
    public class ShipmentController : Controller
    {
        private readonly EFaturaContext _context;

        public ShipmentController(EFaturaContext context)
        {
            _context = context;
        }

        // Listeleme
        public IActionResult Index(string searchString, DateTime? startDate, DateTime? endDate, int page = 1)
        {
            int pageSize = 20; // her sayfada 20 kayıt

            var shipments = _context.Shipments
                .Include(s => s.RecieverCustomer)
                .Include(s => s.SenderUser)
                .Include(s => s.ShipmentItems)
                .ThenInclude(si => si.Product)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                shipments = shipments.Where(s =>
                    s.ShipmentNo.Contains(searchString) ||
                    s.RecieverCustomer.CompanyName.Contains(searchString) ||
                    s.SenderUser.Username.Contains(searchString));
            }

            if (startDate.HasValue)
            {
                shipments = shipments.Where(s => s.ShipmentDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                shipments = shipments.Where(s => s.ShipmentDate <= endDate.Value);
            }

            var totalCount = shipments.Count();
            var result = shipments
                .OrderByDescending(s => s.ShipmentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalCount = totalCount;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;

            return View(result);
        }


        public IActionResult ExportToExcel(int id)
        {
            var shipment = _context.Shipments
                .Include(s => s.RecieverCustomer)
                .Include(s => s.SenderUser)
                .Include(s => s.ShipmentItems)
                .ThenInclude(si => si.Product)
                .FirstOrDefault(s => s.Id == id);

            if (shipment == null)
            {
                return NotFound();
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("İrsaliye Detayı");
                int currentRow = 1;

                // Başlık
                worksheet.Cell(currentRow, 1).Value = "İrsaliye No";
                worksheet.Cell(currentRow, 2).Value = "Müşteri";
                worksheet.Cell(currentRow, 3).Value = "Gönderen Kullanıcı";
                worksheet.Cell(currentRow, 4).Value = "İrsaliye Tarihi";
                worksheet.Cell(currentRow, 5).Value = "Ürün Adı";
                worksheet.Cell(currentRow, 6).Value = "Miktar";
                worksheet.Cell(currentRow, 7).Value = "Birim";

                // İçerik
                foreach (var item in shipment.ShipmentItems)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = shipment.ShipmentNo;
                    worksheet.Cell(currentRow, 2).Value = shipment.RecieverCustomer?.CompanyName;
                    worksheet.Cell(currentRow, 3).Value = shipment.SenderUser?.Username;
                    worksheet.Cell(currentRow, 4).Value = shipment.ShipmentDate.ToString("dd.MM.yyyy");
                    worksheet.Cell(currentRow, 5).Value = item.Product?.Name;
                    worksheet.Cell(currentRow, 6).Value = item.Quantity;
                    worksheet.Cell(currentRow, 7).Value = item.Unit;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Irsaliye_{shipment.ShipmentNo}.xlsx");
                }
            }
        }


        public IActionResult ExportToPdf(int id)
        {
            var shipment = _context.Shipments
                .Include(s => s.RecieverCustomer)
                .Include(s => s.SenderUser)
                .Include(s => s.ShipmentItems)
                .ThenInclude(si => si.Product)
                .FirstOrDefault(s => s.Id == id);

            if (shipment == null)
            {
                return NotFound();
            }

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);

                    page.Header()
                        .Text($"İrsaliye No: {shipment.ShipmentNo}")
                        .SemiBold().FontSize(20).AlignCenter();

                    page.Content()
                        .Column(col =>
                        {
                            col.Item().Text($"Müşteri: {shipment.RecieverCustomer?.CompanyName}");
                            col.Item().Text($"Gönderen Kullanıcı: {shipment.SenderUser?.Username}");
                            col.Item().Text($"İrsaliye Tarihi: {shipment.ShipmentDate:dd.MM.yyyy}");
                            col.Item().Text($"Oluşturulma Tarihi: {shipment.CreateDate:dd.MM.yyyy HH:mm}");

                            col.Item().LineHorizontal(1);

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(200);
                                    columns.ConstantColumn(100);
                                    columns.ConstantColumn(100);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Ürün Adı").Bold();
                                    header.Cell().Text("Miktar").Bold();
                                    header.Cell().Text("Birim").Bold();
                                });

                                foreach (var item in shipment.ShipmentItems)
                                {
                                    table.Cell().Text(item.Product?.Name);
                                    table.Cell().Text(item.Quantity.ToString());
                                    table.Cell().Text(item.Unit);
                                }
                            });
                        });
                });
            });

            var pdf = document.GeneratePdf();
            return File(pdf, "application/pdf", $"Irsaliye_{shipment.ShipmentNo}.pdf");
        }



        // Oluşturma sayfası
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Customers = _context.Customers.ToList();
            ViewBag.Products = _context.Products.ToList();
            return View();
        }

        // Kaydetme
        [HttpPost]
        public IActionResult Create(Shipment shipment)
        {
            if (ModelState.IsValid)
            {
                shipment.ShipmentNo = $"SHP-{DateTime.Now:yyyyMMddHHmmss}";
                shipment.CreateDate = DateTime.Now;
                shipment.SenderUserId = 1; // şimdilik sabit

                _context.Shipments.Add(shipment);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.Customers = _context.Customers.ToList();
            ViewBag.Products = _context.Products.ToList();
            return View(shipment);
        }

        public IActionResult Details(int id)
        {
            var shipment = _context.Shipments
                .Include(s => s.RecieverCustomer)
                .Include(s => s.SenderUser)
                .Include(s => s.ShipmentItems)
                    .ThenInclude(si => si.Product)
                .FirstOrDefault(s => s.Id == id);

            if (shipment == null)
            {
                return NotFound();
            }

            return View(shipment);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var shipment = _context.Shipments
                .Include(s => s.ShipmentItems)
                .ThenInclude(si => si.Product)
                .FirstOrDefault(s => s.Id == id);

            if (shipment == null)
            {
                return NotFound();
            }

            ViewBag.Customers = _context.Customers.ToList();
            ViewBag.Products = _context.Products.ToList();
            return View(shipment);
        }

        [HttpPost]
        public IActionResult Edit(Shipment shipment)
        {
            if (ModelState.IsValid)
            {
                var existingShipment = _context.Shipments
                    .Include(s => s.ShipmentItems)
                    .FirstOrDefault(s => s.Id == shipment.Id);

                if (existingShipment == null)
                {
                    return NotFound();
                }

                // Ana bilgileri güncelle
                existingShipment.RecieverCustomerId = shipment.RecieverCustomerId;
                existingShipment.ShipmentDate = shipment.ShipmentDate;

                // Eski ürünleri sil
                _context.ShipmentItems.RemoveRange(existingShipment.ShipmentItems);

                // Yeni ürünleri ekle
                foreach (var item in shipment.ShipmentItems)
                {
                    item.ShipmentId = existingShipment.Id;
                    _context.ShipmentItems.Add(item);
                }

                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Customers = _context.Customers.ToList();
            ViewBag.Products = _context.Products.ToList();
            return View(shipment);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var shipment = _context.Shipments
                .Include(s => s.ShipmentItems)
                .FirstOrDefault(s => s.Id == id);

            if (shipment == null)
            {
                return NotFound();
            }

            return View(shipment);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var shipment = _context.Shipments
                .Include(s => s.ShipmentItems)
                .FirstOrDefault(s => s.Id == id);

            if (shipment == null)
            {
                return NotFound();
            }

            // Önce irsaliye ürünlerini sil
            _context.ShipmentItems.RemoveRange(shipment.ShipmentItems);

            // Sonra irsaliyeyi sil
            _context.Shipments.Remove(shipment);

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

    }
}
