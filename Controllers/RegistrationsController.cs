using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAn.Data;
using DoAn.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using QRCoder;

namespace DoAn.Controllers
{
    [Authorize]
    public class RegistrationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegistrationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Registrations/MyTickets
        public async Task<IActionResult> MyTickets()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var registrations = await _context.Registrations
                .Include(r => r.Event)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RegistrationDate)
                .ToListAsync();

            var qrCodes = new Dictionary<int, string>();
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                foreach (var reg in registrations)
                {
                    using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(reg.CheckInCode, QRCodeGenerator.ECCLevel.Q))
                    using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                    {
                        byte[] qrCodeImage = qrCode.GetGraphic(10);
                        qrCodes[reg.Id] = Convert.ToBase64String(qrCodeImage);
                    }
                }
            }
            ViewBag.QrCodes = qrCodes;

            return View(registrations);
        }

        // GET: Registrations/Register/5
        [HttpGet]
        public async Task<IActionResult> Register(int eventId)
        {
            var @event = await _context.Events.FindAsync(eventId);
            if (@event == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existing = await _context.Registrations
                .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);

            if (existing != null)
            {
                TempData["ErrorMessage"] = "Bạn đã đăng ký sự kiện này rồi.";
                return RedirectToAction("Details", "Events", new { id = eventId });
            }

            var model = new Registration { EventId = eventId, Event = @event };
            return View(model);
        }

        // POST: Registrations/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Registration model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            // Kiểm tra số lượng tối đa
            var @event = await _context.Events.FindAsync(model.EventId);
            if (@event != null && @event.MaxAttendees.HasValue)
            {
                var count = await _context.Registrations.CountAsync(r => r.EventId == model.EventId);
                if (count >= @event.MaxAttendees.Value)
                {
                    TempData["ErrorMessage"] = "Sự kiện này đã đủ số lượng người đăng ký.";
                    return RedirectToAction("Index", "Events");
                }
            }

            model.UserId = userId;
            model.RegistrationDate = DateTime.Now;
            model.CheckInCode = Guid.NewGuid().ToString();

            // Xóa dữ liệu điều hướng để tránh lỗi Model Binding
            ModelState.Remove("User");
            ModelState.Remove("Event");
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                _context.Registrations.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đăng ký thành công! Chào mừng bạn đến với sự kiện.";
                return RedirectToAction(nameof(MyTickets));
            }

            return View(model);
        }

        // GET: Registrations/Ticket/5
        public async Task<IActionResult> Ticket(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var registration = await _context.Registrations
                .Include(r => r.Event)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id && (r.UserId == userId || User.IsInRole("Admin")));

            if (registration == null) return NotFound();

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(registration.CheckInCode, QRCodeGenerator.ECCLevel.Q))
            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {
                byte[] qrCodeImage = qrCode.GetGraphic(20);
                ViewBag.QrCodeImage = Convert.ToBase64String(qrCodeImage);
            }

            return View(registration);
        }
    }
}
