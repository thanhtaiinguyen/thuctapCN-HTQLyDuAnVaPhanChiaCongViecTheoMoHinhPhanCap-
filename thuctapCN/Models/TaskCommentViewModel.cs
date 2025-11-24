using System.ComponentModel.DataAnnotations;

namespace thuctapCN.Models
{
    public class CreateCommentViewModel
    {
        [Required(ErrorMessage = "Nội dung bình luận là bắt buộc")]
        [StringLength(2000, ErrorMessage = "Bình luận không được vượt quá 2000 ký tự")]
        [Display(Name = "Nội dung bình luận")]
        public string Content { get; set; } = string.Empty;

        public int TaskAssignmentId { get; set; }
    }

    public class EditCommentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nội dung bình luận là bắt buộc")]
        [StringLength(2000, ErrorMessage = "Bình luận không được vượt quá 2000 ký tự")]
        [Display(Name = "Nội dung bình luận")]
        public string Content { get; set; } = string.Empty;

        public int TaskAssignmentId { get; set; }
    }

    public class CommentViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? UserAvatar { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}
