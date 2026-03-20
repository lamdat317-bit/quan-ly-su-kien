using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAn.Models
{
    public class Registration
    {
        public int Id { get; set; }

        [Required]
        public int EventId { get; set; }
        [ForeignKey("EventId")]
        public virtual Event? Event { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Số ghế / Vị trí")]
        public string SeatNumber { get; set; } = "Tự do";

        [Required]
        [Display(Name = "Ngày đăng ký")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [Required]
        public string CheckInCode { get; set; } = Guid.NewGuid().ToString();

        [Display(Name = "Đã điểm danh")]
        public bool IsCheckedIn { get; set; } = false;

        [Display(Name = "Thời gian điểm danh")]
        public DateTime? CheckInTime { get; set; }
    }
}
