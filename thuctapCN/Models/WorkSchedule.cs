using System.ComponentModel.DataAnnotations;

namespace thuctapCN.Models
{
    /// <summary>
    /// Lịch làm việc - Admin/Manager xếp lịch cho nhân viên
    /// </summary>
    public class WorkSchedule
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public int? ProjectId { get; set; }

        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(200)]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
        [Display(Name = "Ngày bắt đầu")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
        [Display(Name = "Ngày kết thúc")]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Cả ngày")]
        public bool IsAllDay { get; set; } = false;

        [Display(Name = "Màu sắc")]
        public string Color { get; set; } = "#3788d8";

        [Display(Name = "Loại lịch")]
        public string ScheduleType { get; set; } = "Work"; // Work, Meeting, Holiday, Other

        [Display(Name = "Địa điểm")]
        [StringLength(200)]
        public string? Location { get; set; }

        [Required]
        public string CreatedByUserId { get; set; } = string.Empty;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedDate { get; set; }

        // Navigation
        public ApplicationUser User { get; set; } = null!;
        public ApplicationUser CreatedByUser { get; set; } = null!;
        public Project? Project { get; set; }
    }
}
