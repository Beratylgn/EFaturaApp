using Microsoft.AspNetCore.Mvc;
using EFaturaApp.Data;
using EFaturaApp.Models;
using System.Security.Cryptography;

namespace EFaturaApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly EFaturaContext _context;

        public ProductController(EFaturaContext context)
        {
            _context = context;
        }

        // ----------------- Helper: Benzersiz stok kodu üret -----------------
        private string GenerateUniqueStockCode(int length = 8, string prefix = "PRD-")
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // 0,1,I,O yok
            string NewCode()
            {
                var bytes = new byte[length];
                RandomNumberGenerator.Fill(bytes);
                var chars = new char[length];
                for (int i = 0; i < length; i++)
                    chars[i] = alphabet[bytes[i] % alphabet.Length];
                return prefix + new string(chars);
            }

            string code = NewCode();
            // Çakışma olursa tekrar dene
            while (_context.Products.Any(p => p.StockCode == code))
                code = NewCode();

            return code;
        }
        // --------------------------------------------------------------------

        // Listeleme (Index)
        public IActionResult Index(string searchString, int pageNumber = 1, int pageSize = 5)
        {
            var products = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p =>
                    p.Name.Contains(searchString) || p.StockCode.Contains(searchString));
            }

            var totalRecords = products.Count();
            var items = products
                .OrderBy(p => p.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.SearchString = searchString;

            return View(items);
        }

        // GET: /Product/Create
        [HttpGet]
        public IActionResult Create()
        {
            // Forma default bir stok kodu verelim
            var model = new Product
            {
                StockCode = GenerateUniqueStockCode(),
                Currency = "TRY",
                Unit = "Adet",
                CreateDate = DateTime.Now
            };
            return View(model);
        }

        // POST: /Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product)
        {
            // Kod boş bırakılmışsa ya da çakışıyorsa benzersiz üret
            if (string.IsNullOrWhiteSpace(product.StockCode) ||
                _context.Products.Any(p => p.StockCode == product.StockCode))
            {
                product.StockCode = GenerateUniqueStockCode();
            }

            if (ModelState.IsValid)
            {
                product.CreateDate ??= DateTime.Now;
                product.Currency ??= "TRY";
                product.Unit ??= "Adet";

                _context.Products.Add(product);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: /Product/Edit/5
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: /Product/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product product)
        {
            // Kendi kaydı dışındaki çakışmaları kontrol et
            if (_context.Products.Any(p => p.StockCode == product.StockCode && p.Id != product.Id))
            {
                ModelState.AddModelError("StockCode", "Bu stok kodu başka bir üründe zaten mevcut!");
            }

            if (ModelState.IsValid)
            {
                var existingProduct = _context.Products.FirstOrDefault(p => p.Id == product.Id);
                if (existingProduct == null) return NotFound();

                existingProduct.Name = product.Name;
                existingProduct.UnitQuantity = product.UnitQuantity;
                existingProduct.Unit = product.Unit;
                existingProduct.UnitPrice = product.UnitPrice;
                existingProduct.Currency = product.Currency;
                existingProduct.StockCode = product.StockCode;

                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: /Product/Delete/5
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: /Product/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
