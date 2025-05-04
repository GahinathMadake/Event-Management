using EventManagement.Models;
using EventManagement.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace EventManagement.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewController(ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: Review/Create
        public IActionResult Create()
        {
            return View();
        }


        // POST: Review/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Review review)
        {
            // ✅ Get Email from ClaimsPrincipal (i.e., from cookie authentication)
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Account"); // User not logged in
            }

            // ✅ Get the user from DB based on email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return RedirectToAction("Login", "Account"); // User not found in DB
            }

            if (ModelState.IsValid)
            {
                review.UserId = user.Id;
                review.User = user;

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Home"); // Or a success page
            }

            return View(review); // If model validation fails
        }
       
    }
}
