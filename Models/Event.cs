using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sự kiện")]
        [StringLength(200)]
        [Display(Name = "Tên sự kiện")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa điểm")]
        [Display(Name = "Địa điểm")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn thời gian bắt đầu")]
        [Display(Name = "Bắt đầu")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thời gian kết thúc")]
        [Display(Name = "Kết thúc")]
        public DateTime EndTime { get; set; }

        [Display(Name = "Số lượng tối đa")]
        public int? MaxAttendees { get; set; }

        [Display(Name = "Ảnh sự kiện")]
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    }
}
