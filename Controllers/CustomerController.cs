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
        public IActionResult Index()
        {
            var customers = _context.Customers.ToList();
            return View(customers);
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
    }
}
