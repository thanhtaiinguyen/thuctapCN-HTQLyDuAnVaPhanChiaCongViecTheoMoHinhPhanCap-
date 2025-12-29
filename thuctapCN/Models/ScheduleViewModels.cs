using System.ComponentModel.DataAnnotations;

namespace thuctapCN.Models
{
    public class NotificationViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Type { get; set; } = "Info";
        public string? RelatedUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedDate { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
    }
    public class CreateNotificationViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn người nhận")]
        [Display(Name = "Người nhận")]
        public string UserId { get; set; } = string.Empty;

        [Display(Name = "Gửi cho tất cả")]
        public bool SendToAll { get; set; } = false;

        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề không quá 200 ký tự")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung là bắt buộc")]
        [StringLength(2000, ErrorMessage = "Nội dung không quá 2000 ký tự")]
        [Display(Name = "Nội dung")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Loại thông báo")]
        public string Type { get; set; } = "Info";

        [Display(Name = "Đường dẫn liên quan")]
        public string? RelatedUrl { get; set; }

        public List<ApplicationUser> AvailableUsers { get; set; } = new();
    }
    public class ScheduleViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? UserAvatar { get; set; }
        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsAllDay { get; set; }
        public string Color { get; set; } = "#3788d8";
        public string ScheduleType { get; set; } = "Work";
        public string? Location { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
    public class CreateScheduleViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn nhân viên")]
        [Display(Name = "Nhân viên")]
        public string UserId { get; set; } = string.Empty;

        [Display(Name = "Dự án liên quan")]
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
        public DateTime StartDate { get; set; } = DateTime.Today.AddHours(9);

        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
        [Display(Name = "Ngày kết thúc")]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; } = DateTime.Today.AddHours(18);

        [Display(Name = "Cả ngày")]
        public bool IsAllDay { get; set; } = false;

        [Display(Name = "Màu sắc")]
        public string Color { get; set; } = "#3788d8";

        [Display(Name = "Loại lịch")]
        public string ScheduleType { get; set; } = "Work";

        [StringLength(200)]
        [Display(Name = "Địa điểm")]
        public string? Location { get; set; }

        // Dropdown data
        public List<ApplicationUser> AvailableUsers { get; set; } = new();
        public List<Project> AvailableProjects { get; set; } = new();
    }
    public class CalendarEventViewModel
    {
        public int id { get; set; }
        public string title { get; set; } = string.Empty;
        public string start { get; set; } = string.Empty;
        public string end { get; set; } = string.Empty;
        public bool allDay { get; set; }
        public string color { get; set; } = "#3788d8";
        public string url { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
    }
}
