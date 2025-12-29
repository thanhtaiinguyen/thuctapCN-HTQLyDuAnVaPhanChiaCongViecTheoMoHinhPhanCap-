using System.ComponentModel.DataAnnotations;

namespace thuctapCN.Models
{
    /// <summary>
    /// Thông báo gửi cho người dùng
    /// </summary>
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề không quá 200 ký tự")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung là bắt buộc")]
        [StringLength(2000, ErrorMessage = "Nội dung không quá 2000 ký tự")]
        [Display(Name = "Nội dung")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Loại thông báo")]
        public string Type { get; set; } = "Info"; // Info, Warning, Success, Danger

        [Display(Name = "Đường dẫn liên quan")]
        public string? RelatedUrl { get; set; }

        [Display(Name = "Đã đọc")]
        public bool IsRead { get; set; } = false;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation
        public ApplicationUser User { get; set; } = null!;
    }
}
