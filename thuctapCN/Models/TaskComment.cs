using System.ComponentModel.DataAnnotations;

namespace thuctapCN.Models
{
    public class TaskComment
    {
        public int Id { get; set; }

        [Required]
        public int TaskAssignmentId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung bình luận là bắt buộc")]
        [StringLength(2000, ErrorMessage = "Bình luận không được vượt quá 2000 ký tự")]
        [Display(Name = "Nội dung bình luận")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public TaskAssignment TaskAssignment { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}
