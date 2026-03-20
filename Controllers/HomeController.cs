using DoAn.Data;
using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net.Http;

namespace DoAn.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy IP Toàn cầu để hiện lên web cho bạn dễ copy link
            try 
            {
                using var client = new HttpClient();
                var publicIp = await client.GetStringAsync("https://api.ipify.org");
                ViewBag.PublicIp = publicIp;
            }
            catch { ViewBag.PublicIp = "Chưa xác định"; }

            if (User.IsInRole("Admin"))
            {
                ViewBag.TotalEvents = await _context.Events.CountAsync();
                ViewBag.TotalRegistrations = await _context.Registrations.CountAsync();
                ViewBag.CheckedInCount = await _context.Registrations.CountAsync(r => r.IsCheckedIn);
                
                var recentCheckins = await _context.Registrations
                    .Include(r => r.User)
                    .Include(r => r.Event)
                    .Where(r => r.IsCheckedIn)
                    .OrderByDescending(r => r.CheckInTime)
                    .Take(5)
                    .ToListAsync();
                
                return View("AdminDashboard", recentCheckins);
            }

            return View();
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
    }
}
