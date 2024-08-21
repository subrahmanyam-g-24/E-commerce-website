using Ecommerce_App_prac1.Models;
using Ecommerce_App_prac1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_App_prac1.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Login));
            }
            return View(user);
        }

        public IActionResult Login(string username, string password)
        {
            if (HttpContext.Request.Method == "POST")
            {
                var user = _context.Users.AsEnumerable().FirstOrDefault(u => string.Equals(u.UserName, username, StringComparison.Ordinal) 
                                                                          && string.Equals(u.Password, password, StringComparison.Ordinal)); 
                if (user != null)
                {
                    // Store user information in session or use cookies
                    HttpContext.Session.SetString("Username", username);
                    return RedirectToAction("Index", "Products");
                }
                ModelState.AddModelError("", "Invalid login details");
            }
            return View();
        }

    }
}
