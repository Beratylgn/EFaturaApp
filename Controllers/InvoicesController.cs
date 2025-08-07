using EFaturaApp.Data;
using EFaturaApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EFaturaApp.Controllers
{
    public class InvoicesController : Controller
    {
        private readonly EFaturaContext _context;
        private const int PageSize = 20;

        public InvoicesController(EFaturaContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Customers = _context.CUSTOMERS.ToList();
            ViewBag.Products = _context.PRODUCT.ToList();
            return View();
        }

        public async Task<IActionResult> Index(string searchString, DateTime? startDate, DateTime? endDate, int page = 1)
        {
            var query = _context.INVOICES
                .Include(i => i.ReceiverCustomer)
                .AsQueryable();

            // Arama filtresi
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(i => i.INVOICENO.Contains(searchString));
            }

            // Tarih aralığı filtresi
            if (startDate.HasValue)
            {
                query = query.Where(i => i.INVOICEDATE >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(i => i.INVOICEDATE <= endDate.Value);
            }

            // Toplam kayıt sayısı
            var totalRecords = await query.CountAsync();

            // Sayfalama
            var invoices = await query
                .OrderByDescending(i => i.INVOICEDATE)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // ViewBag ile verileri View'a taşı
            ViewBag.TotalRecords = totalRecords;
            ViewBag.Page = page;
            ViewBag.PageSize = PageSize;
            ViewBag.SearchString = searchString;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View(invoices);
        }

        // POST: Invoices/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Invoice invoice, List<InvoiceItem> items)
        {
            Console.WriteLine("🔥🔥🔥 Create POST METODU ÇALIŞTI 🔥🔥🔥");

            // Tüm form verilerini logla
            Console.WriteLine("🔎 Request verileri:");
            foreach (var key in Request.Form.Keys)
            {
                Console.WriteLine($"📝 {key} = {Request.Form[key]}");
            }

            Console.WriteLine($"Fatura No: {invoice.INVOICENO}");
            Console.WriteLine($"Fatura Tarihi: {invoice.INVOICEDATE}");
            Console.WriteLine($"Müşteri ID: {invoice.RECEIVERCUSTOMERID}");
            Console.WriteLine($"Kalem Sayısı: {items?.Count}");

            if (ModelState.IsValid)
            {
                invoice.CREATEDATE = DateTime.Now;
                invoice.TOTALAMOUNT = items.Sum(i => i.QUANTITY * i.UNITPRICE * (1 + (i.TAXRATE / 100m)));

                _context.INVOICES.Add(invoice);
                _context.SaveChanges();

                foreach (var item in items)
                {
                    item.INVOICEID = invoice.ID;
                    _context.INVOICESITEMS.Add(item);
                }

                _context.SaveChanges();
                Console.WriteLine("✅ Kayıt başarıyla tamamlandı.");
                return RedirectToAction(nameof(Index));
            }
            else
            {
                Console.WriteLine("❌ ModelState geçersiz.");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    foreach (var error in state.Errors)
                    {
                        Console.WriteLine($"🔴 {key}: {error.ErrorMessage}");
                    }
                }
            }

            ViewBag.Customers = _context.CUSTOMERS.ToList();
            ViewBag.Products = _context.PRODUCT.ToList();
            return View(invoice);
        }
    



        // GET: Invoices/Edit/5
        public IActionResult Edit(int id)
        {
            var invoice = _context.INVOICES
                .Include(i => i.InvoiceItems)
                .FirstOrDefault(i => i.ID == id);

            if (invoice == null) return NotFound();

            ViewBag.Customers = _context.CUSTOMERS.ToList();
            ViewBag.Products = _context.PRODUCT.ToList();
            return View(invoice);
        }

        // POST: Invoices/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Invoice invoice, List<InvoiceItem> items)
        {
            if (id != invoice.ID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    invoice.TOTALAMOUNT = items.Sum(i => i.QUANTITY * i.UNITPRICE * (1 + (i.TAXRATE / 100)));
                    _context.Update(invoice);

                    // Eski kalemleri sil
                    var oldItems = _context.INVOICESITEMS.Where(x => x.INVOICEID == invoice.ID).ToList();
                    _context.INVOICESITEMS.RemoveRange(oldItems);

                    // Yeni kalemleri ekle
                    foreach (var item in items)
                    {
                        item.INVOICEID = invoice.ID;
                        _context.INVOICESITEMS.Add(item);
                    }

                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }
                return RedirectToAction(nameof(Index));
            }
            return View(invoice);
        }

        // GET: Invoices/Details/5
        public IActionResult Details(int id)
        {
            var invoice = _context.INVOICES
                .Include(i => i.ReceiverCustomer)
                .Include(i => i.InvoiceItems)
                    .ThenInclude(ii => ii.Product)
                .FirstOrDefault(i => i.ID == id);

            if (invoice == null) return NotFound();

            return View(invoice);
        }

        // GET: Invoices/Delete/5
        public IActionResult Delete(int id)
        {
            var invoice = _context.INVOICES
                .Include(i => i.ReceiverCustomer)
                .FirstOrDefault(i => i.ID == id);

            if (invoice == null) return NotFound();

            return View(invoice);
        }

        // POST: Invoices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var invoice = _context.INVOICES.Find(id);
            if (invoice != null)
            {
                var items = _context.INVOICESITEMS.Where(ii => ii.INVOICEID == id);
                _context.INVOICESITEMS.RemoveRange(items);
                _context.INVOICES.Remove(invoice);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ExportExcel()
        {
            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Faturalar");
                var currentRow = 1;

                // Başlıklar
                worksheet.Cell(currentRow, 1).Value = "Fatura No";
                worksheet.Cell(currentRow, 2).Value = "Müşteri";
                worksheet.Cell(currentRow, 3).Value = "Fatura Tarihi";
                worksheet.Cell(currentRow, 4).Value = "Toplam Tutar";

                // Satırlar
                var invoices = _context.INVOICES.Include(i => i.ReceiverCustomer).ToList();
                foreach (var invoice in invoices)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = invoice.INVOICENO;
                    worksheet.Cell(currentRow, 2).Value = invoice.ReceiverCustomer?.Name;
                    worksheet.Cell(currentRow, 3).Value = invoice.INVOICEDATE?.ToString("dd/MM/yyyy");
                    worksheet.Cell(currentRow, 4).Value = invoice.TOTALAMOUNT;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Faturalar.xlsx");
                }
            }
        }

        public IActionResult ExportPdf()
        {
            using (var ms = new MemoryStream())
            {
                var writer = new iText.Kernel.Pdf.PdfWriter(ms);
                var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
                var document = new iText.Layout.Document(pdf);

                document.Add(new iText.Layout.Element.Paragraph("Fatura Listesi")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFontSize(18));

                var table = new iText.Layout.Element.Table(4);
                table.AddHeaderCell("Fatura No");
                table.AddHeaderCell("Müşteri");
                table.AddHeaderCell("Fatura Tarihi");
                table.AddHeaderCell("Toplam Tutar");

                var invoices = _context.INVOICES.Include(i => i.ReceiverCustomer).ToList();
                foreach (var invoice in invoices)
                {
                    table.AddCell(invoice.INVOICENO);
                    table.AddCell(invoice.ReceiverCustomer?.Name ?? "-");
                    table.AddCell(invoice.INVOICEDATE?.ToString("dd/MM/yyyy"));
                    (ViewBag.StartDate as DateTime?)?.ToString("yyyy-MM-dd");
                }

                document.Add(table);
                document.Close();

                return File(ms.ToArray(), "application/pdf", "Faturalar.pdf");
            }
        }




    }
}
