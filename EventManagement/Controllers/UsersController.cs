using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EventManagement.Models;
using EventManagement.Data;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;


namespace EventManagement.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }



        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Users"); // or wherever your login page is
        }


        // GET: /Users/Register
        public IActionResult Register()      //get action
        {
            return View(new Signup
            {
                FullName = string.Empty,
                Email = string.Empty,
                Password = string.Empty,
                ConfirmPassword = string.Empty
            });
        }




        // POST: /Users/Register
       [HttpPost]
       [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Signup model)
        {
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email is already registered.");
                    return View(model);
                }

                // Map Signup model to User entity
                var user = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = model.Password, // Your hashing logic
                    CreatedAt = DateTime.UtcNow,
                    Role = "User"
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return RedirectToAction("Login");
            }

            return View(model);
        }


        // GET: /Users/Login
        public IActionResult Login()
        {
            ModelState.Clear(); 
            ModelState.Clear(); // clear form values
           var emptyModel = new Login
           {
               Email = string.Empty,
               Password = string.Empty
           };
           TryValidateModel(emptyModel); // make sure model is revalidated
           return View(emptyModel);

        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null || user.Password != password)
            {
                ModelState.Clear();
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(new Login
                {
                    Email = string.Empty,
                    Password = string.Empty
                });
            }

            HttpContext.Session.SetInt32("UserId", user.Id);


            // ✅ Create claims (you can customize this)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName), 
                new Claim(ClaimTypes.Email, user.Email)
            };

            // ✅ Create identity and sign in
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );

            return RedirectToAction("Index", "Events");
            }
        }
        

}
