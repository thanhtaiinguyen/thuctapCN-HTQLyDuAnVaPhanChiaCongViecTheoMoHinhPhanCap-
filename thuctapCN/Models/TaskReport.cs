using System.ComponentModel.DataAnnotations;

namespace thuctapCN.Models
{
    /// <summary>
    /// Báo cáo công việc - Nhân viên gửi cho Manager
    /// </summary>
    public class TaskReport
    {
        public int Id { get; set; }

        [Required]
        public int TaskAssignmentId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tiêu đề báo cáo là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề không quá 200 ký tự")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung báo cáo là bắt buộc")]
        [StringLength(5000, ErrorMessage = "Nội dung không quá 5000 ký tự")]
        [Display(Name = "Nội dung báo cáo")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "File đính kèm")]
        public string? AttachmentPath { get; set; }

        [Display(Name = "Tiến độ hiện tại (%)")]
        [Range(0, 100)]
        public int CurrentProgress { get; set; }

        [Display(Name = "Trạng thái đã đọc")]
        public bool IsRead { get; set; } = false;

        [Display(Name = "Ngày gửi")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public TaskAssignment TaskAssignment { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}
