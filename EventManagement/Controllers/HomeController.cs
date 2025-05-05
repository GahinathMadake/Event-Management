using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EventManagement.Models;
using EventManagement.Data;
using Microsoft.EntityFrameworkCore; 
using EventManagement.ViewModels;



namespace EventManagement.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private readonly ApplicationDbContext _context;

    // ✅ Constructor injection for context & logger
    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    // public IActionResult Index()
    // {
    //     return View();
    // }


    public async Task<IActionResult> Index()
    {
        var totalEvents = _context.UserEvents
            .Include(ue => ue.Event)
            .Count(ue => ue.Event != null && ue.Event.Date <= DateTime.UtcNow);

        var totalUsers = _context.Users.Count();

        var expertOrganizers = _context.Users
            .Count(u => u.Events.Any()); // Users who created events

        var citiesCovered = _context.Users
            .Select(u => u.Events.Select(e => e.Location))
            .SelectMany(e => e)
            .Distinct()
            .Count();

        var topReviews = await _context.Reviews// Filter by 5-star rating
            .OrderByDescending(r => r.Date) // Optional: Order by creation date or rating
            .Take(100) // Fetch only the top 100 reviews
            .Include(r => r.User) // Include User data if needed
            .ToListAsync(); // Execute the query


        var viewModel = new AnalyticsViewModel
        {
            TotalEvents = totalEvents,
            TotalUsers = totalUsers,
            ExpertOrganizers = expertOrganizers,
            CitiesCovered = citiesCovered,
            Reviews = topReviews,
        };

        return View(viewModel);
    }


    public IActionResult Privacy()
    {
        return View();
    }

    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // GET: /Service
    public IActionResult Service()
    {
        return View();
    }

}