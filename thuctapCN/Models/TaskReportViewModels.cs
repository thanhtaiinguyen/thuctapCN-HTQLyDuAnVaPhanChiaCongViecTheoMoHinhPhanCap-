using System.ComponentModel.DataAnnotations;

namespace thuctapCN.Models
{
    /// <summary>
    /// ViewModel để gửi báo cáo mới
    /// </summary>
    public class CreateReportViewModel
    {
        public int TaskAssignmentId { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tiêu đề báo cáo là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề không quá 200 ký tự")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung báo cáo là bắt buộc")]
        [StringLength(5000, ErrorMessage = "Nội dung không quá 5000 ký tự")]
        [Display(Name = "Nội dung báo cáo")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Tiến độ hiện tại (%)")]
        [Range(0, 100, ErrorMessage = "Tiến độ phải từ 0 đến 100")]
        public int CurrentProgress { get; set; }

        [Display(Name = "File đính kèm")]
        public IFormFile? AttachmentFile { get; set; }
    }

    /// <summary>
    /// ViewModel hiển thị danh sách báo cáo
    /// </summary>
    public class ReportListViewModel
    {
        public int Id { get; set; }
        public int TaskAssignmentId { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? UserAvatar { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ContentPreview { get; set; } = string.Empty;
        public int CurrentProgress { get; set; }
        public bool IsRead { get; set; }
        public bool HasAttachment { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// ViewModel xem chi tiết báo cáo
    /// </summary>
    public class ReportDetailsViewModel
    {
        public int Id { get; set; }
        public int TaskAssignmentId { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectCode { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? UserAvatar { get; set; }
        public string UserEmployeeCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int CurrentProgress { get; set; }
        public string? AttachmentPath { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool CanDelete { get; set; }
    }
}
