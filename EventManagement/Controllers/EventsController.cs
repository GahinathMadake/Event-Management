using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using EventManagement.Data;
using EventManagement.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using EventManagement.ViewModels;



namespace EventManagement.Controllers
{
    [Authorize]
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }



        public async Task<IActionResult> Index()
{
    var email = User.FindFirstValue(ClaimTypes.Email);
    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    if (user == null)
        return Unauthorized();

    // 🔐 Only fetch events created by this user
    var userEvents = await _context.Events
                                   .Where(e => e.UserId == user.Id)
                                   .ToListAsync();

    return View(userEvents);
}


          // 🔹 GET: /Events/Display (List all events)
        public async Task<IActionResult> Display()
        {
            var events = await _context.Events.ToListAsync();
            return View("Display", events);

        }


        // 🔹 GET: /Events/Details/5 (View event details)
        // public async Task<IActionResult> Details(int? id)
        // {
        //     if (id == null) return NotFound();

        //     var ev = await _context.Events.FindAsync(id);
        //     if (ev == null) return NotFound();
    
        //     return View(ev);
        // }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            var registeredUsers = await _context.UserEvents
                .Where(ue => ue.EventId == id)
                .Include(ue => ue.User)
                .Select(ue => ue.User!)
                .ToListAsync();

            var viewModel = new EventDetailsViewModel
            {
                Event = ev,
                RegisteredUsers = registeredUsers
            };

            return View(viewModel);
        }



        

        // 🔹 GET: /Events/Create (Form to create event)
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Date,Location,Capacity")] Event ev)
        {
            if (ModelState.IsValid)
            {
                // Get logged-in user's email
                var email = User.FindFirstValue(ClaimTypes.Email);
                var user = _context.Users.FirstOrDefault(u => u.Email == email);

                if (user == null)
                {
                    return Unauthorized(); // just in case user is not found
                }

                ev.UserId = user.Id; // ✅ Set the foreign key properly

                _context.Add(ev);
                await _context.SaveChangesAsync();


                return RedirectToAction(nameof(Index));
            }

            return View(ev);
        }




        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            // 🔐 Get current user
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            // 🔐 Authorization check
            if (ev.UserId != user?.Id)
                return Forbid(); // or return Unauthorized();

            return View(ev);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Date,Location,Capacity")] Event ev)
        {
            if (id != ev.Id) return NotFound();

            var existingEvent = await _context.Events.FindAsync(id);
            if (existingEvent == null) return NotFound();

            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            // 🔐 Authorization check
            if (existingEvent.UserId != user?.Id)
                return Forbid();

            // Update allowed fields
            existingEvent.Title = ev.Title;
            existingEvent.Description = ev.Description;
            existingEvent.Date = ev.Date;
            existingEvent.Location = ev.Location;
            existingEvent.Capacity = ev.Capacity;

            _context.Update(existingEvent);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (ev.UserId != user?.Id)
                return Forbid();

            return View(ev);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (ev.UserId != user?.Id)
                return Forbid();

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> RegisterEvent(int id)
        {

            var eventToRegister = await _context.Events.FindAsync(id);

            if (eventToRegister == null)
            {
                return NotFound("Event not found.");
            }

            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Users");
            }

            // Get user email from claims
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            // Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var userId = user.Id;

            // Check if already registered
            var alreadyRegistered = await _context.UserEvents
                .AnyAsync(ue => ue.EventId == id && ue.UserId == userId);

            if (alreadyRegistered)
            {
                TempData["Message"] = "You have already registered for this event.";
                var events = await _context.Events.ToListAsync();
                return View("Display", events);

                // return RedirectToAction("Events", "Display");
            }

            var registration = new UserEvent
            {
                EventId = id,
                UserId = userId,
                RegisteredAt = DateTime.UtcNow,
                Status = "Registered"
            };

            _context.UserEvents.Add(registration);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Successfully registered!";
            return RedirectToAction("MyRegisteredEvents");
        }


        public async Task<IActionResult> Attendees(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            var eventEntity = await _context.Events
                .Include(e => e.UserEvents)
                .ThenInclude(ue => ue.User)
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (eventEntity == null)
                return NotFound("Event not found or not owned by you.");

            return View(eventEntity);
        }


        public async Task<IActionResult> MyRegisteredEvents(){
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var registeredEvents = await _context.UserEvents
                .Include(ue => ue.Event)
                .Where(ue => ue.UserId == userId)
                .ToListAsync();

            return View(registeredEvents);
        }

        public async Task<IActionResult> PublicDetails(int? id){
            if (id == null) return NotFound();

                    var ev = await _context.Events.FindAsync(id);
                    if (ev == null) return NotFound();
                    var events = await _context.Events.ToListAsync();
                    return View("Display", events);

        }

     }
 }
