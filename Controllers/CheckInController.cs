using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAn.Data;
using DoAn.Models;
using Microsoft.AspNetCore.Authorization;

namespace DoAn.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class CheckInController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CheckInController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CheckIn
        public IActionResult Scan()
        {
            return View();
        }

        // POST: CheckIn/Verify
        [HttpPost]
        public async Task<IActionResult> Verify(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return Json(new { success = false, message = "Mã QR không hợp lệ." });
            }

            var registration = await _context.Registrations
                .Include(r => r.Event)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.CheckInCode == code);

            if (registration == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin đăng ký cho mã này." });
            }

            if (registration.IsCheckedIn)
            {
                return Json(new { 
                    success = false, 
                    message = $"Người dùng {registration.User?.FullName} đã điểm danh vào lúc {registration.CheckInTime:HH:mm dd/MM/yyyy}." 
                });
            }

            // Mark as checked in
            registration.IsCheckedIn = true;
            registration.CheckInTime = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { 
                    success = true, 
                    message = $"Đã điểm danh cho: {registration.User?.FullName}",
                    eventTitle = registration.Event?.Title,
                    userName = registration.User?.FullName,
                    phoneNumber = registration.PhoneNumber,
                    seatNumber = registration.SeatNumber,
                    checkInTime = registration.CheckInTime?.ToString("HH:mm:ss dd/MM/yyyy")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // GET: CheckIn/History
        public async Task<IActionResult> History()
        {
            var history = await _context.Registrations
                .Include(r => r.Event)
                .Include(r => r.User)
                .Where(r => r.IsCheckedIn)
                .OrderByDescending(r => r.CheckInTime)
                .ToListAsync();
            return View(history);
        }
    }
}
