using Microsoft.AspNetCore.Mvc;
using EFaturaApp.Data;
using EFaturaApp.Models;
using System.Linq;

namespace EFaturaApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly EFaturaContext _context;

        public AccountController(EFaturaContext context)
        {
            _context = context;
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            // Kullanıcıyı veritabanında kontrol ediyoruz
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                // Kullanıcı bulunduysa session açıyoruz
                HttpContext.Session.SetInt32("UserId", user.Id);       // Kullanıcının ID'si
                HttpContext.Session.SetString("Fullname", user.Fullname); // Kullanıcının adı

                TempData["Message"] = $"Hoş geldin {user.Fullname}!";
                return RedirectToAction("Index", "Home");
            }

            // Kullanıcı bulunamazsa hata mesajı
            ViewBag.Error = "Kullanıcı adı veya şifre yanlış!";
            return View();
        }


        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            user.CreateDate = DateTime.Now;
            _context.Users.Add(user);
            _context.SaveChanges();
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            // Tüm session bilgilerini siliyoruz
            HttpContext.Session.Clear();

            // Kullanıcıyı giriş sayfasına yönlendiriyoruz
            return RedirectToAction("Login");
        }

    }
}
