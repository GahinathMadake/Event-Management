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
using EventManagement.Services;



namespace EventManagement.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly EmailService _emailService;

        public UsersController(EmailService emailService, ApplicationDbContext context)
        {
            _emailService = emailService;
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



        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            // Fetch user like in Login
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            // Handle invalid user
            if (user == null)
            {
                ModelState.Clear();
                ModelState.AddModelError(string.Empty, "No user found with this email.");
                return View();
            }

            // Generate secure token and set expiry
            user.PasswordResetToken = Guid.NewGuid().ToString();
            user.ResetTokenExpiration = DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync();

            // Build reset link
            var resetLink = Url.Action("ResetPassword", "Users", new
            {
                email = user.Email,
                token = user.PasswordResetToken
            }, Request.Scheme);

            // Send reset email
            await _emailService.SendEmail(
                user.Email,
                "Reset Password",
                $"Hello {user.FullName},<br/><br/>Click <a href='{resetLink}'>here</a> to reset your password.<br/><br/>This link will expire in 1 hour."
            );
            // Feedback
            TempData["Message"] = "A password reset link has been sent to your email.";
            return RedirectToAction("Login");
        }

        
        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            return View(new ResetPasswordViewModel { Email = email, Token = token });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var user = _context.Users.FirstOrDefault(u =>
                u.Email == model.Email &&
                u.PasswordResetToken == model.Token &&
                u.ResetTokenExpiration > DateTime.UtcNow);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid or expired token.");
                return View(model);
            }

            // Update password
            user.Password = model.NewPassword; // You should hash this
            user.PasswordResetToken = null;
            user.ResetTokenExpiration = null;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Password has been reset successfully.";
            return RedirectToAction("Login");
        }


    }

}
