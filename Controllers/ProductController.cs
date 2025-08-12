using Microsoft.AspNetCore.Mvc;
using EFaturaApp.Data;
using EFaturaApp.Models;

namespace EFaturaApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly EFaturaContext _context;

        public ProductController(EFaturaContext context)
        {
            _context = context;
        }

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
            return View();
        }

        // POST: /Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product)
        {

            if (_context.Products.Any(p => p.StockCode == product.StockCode))
            {
                ModelState.AddModelError("StockCode", "Bu stok kodu zaten mevcut!");
            }

            if (ModelState.IsValid)
            {
                // Varsayılan değer atamaları
                product.CreateDate = DateTime.Now;
                if (string.IsNullOrEmpty(product.Currency))
                    product.Currency = "TRY"; // Varsayılan TL
                if (string.IsNullOrEmpty(product.Unit))
                    product.Unit = "Adet";   // Varsayılan Adet

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
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: /Product/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product product)
        {

            if (_context.Products.Any(p => p.StockCode == product.StockCode))
            {
                ModelState.AddModelError("StockCode", "Bu stok kodu zaten mevcut!");
            }


            if (ModelState.IsValid)
            {
                var existingProduct = _context.Products.FirstOrDefault(p => p.Id == product.Id);
                if (existingProduct == null)
                {
                    return NotFound();
                }

                existingProduct.Name = product.Name;
                existingProduct.UnitQuantity = product.UnitQuantity; 
                existingProduct.Unit = product.Unit;
                existingProduct.UnitPrice = product.UnitPrice;
                existingProduct.Currency = product.Currency;

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
            if (product == null)
            {
                return NotFound();
            }
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
