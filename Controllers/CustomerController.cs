using Microsoft.AspNetCore.Mvc;
using EFaturaApp.Data;
using EFaturaApp.Models;

namespace EFaturaApp.Controllers
{
    public class CustomerController : Controller
    {
        private readonly EFaturaContext _context;

        public CustomerController(EFaturaContext context)
        {
            _context = context;
        }

        // Listeleme (Index)
        public IActionResult Index(string searchString, int pageNumber = 1, int pageSize = 5)
        {
            var customers = _context.Customers.AsQueryable();

            // Arama
            if (!string.IsNullOrEmpty(searchString))
            {
                customers = customers.Where(c => c.CompanyName.Contains(searchString)
                                              || c.Email.Contains(searchString));
            }

            // Toplam kayıt sayısı
            var totalRecords = customers.Count();

            // Sayfalama
            var items = customers
                .OrderBy(c => c.CompanyName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.SearchString = searchString;

            return View(items);
        }


        // GET: /Customer/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                customer.CreatedAt = DateTime.Now; // otomatik kayıt tarihi
                _context.Customers.Add(customer);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: /Customer/Edit/5
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.Id == id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: /Customer/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Customer customer)
        {
            if (ModelState.IsValid)
            {
                var existingCustomer = _context.Customers.FirstOrDefault(c => c.Id == customer.Id);
                if (existingCustomer == null)
                {
                    return NotFound();
                }

                // Sadece değişen alanları güncelle
                existingCustomer.CompanyName = customer.CompanyName;
                existingCustomer.TaxNumber = customer.TaxNumber;
                existingCustomer.Address = customer.Address;
                existingCustomer.Email = customer.Email;

                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(customer);
        }


        // GET: /Customer/Delete/5
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.Id == id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: /Customer/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.Id == id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
