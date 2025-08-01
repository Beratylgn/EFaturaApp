using Microsoft.AspNetCore.Mvc;

namespace EFaturaApp.Controllers
{
    public class CustomerController : Controller
    {
        // GET: /Customer/Index
        public IActionResult Index()
        {
            return View();
        }
    }
}
