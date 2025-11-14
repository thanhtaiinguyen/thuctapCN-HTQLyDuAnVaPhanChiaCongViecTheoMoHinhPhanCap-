using System.ComponentModel.DataAnnotations;

namespace thuctapCN.Models
{
    public class TaskAssignment
    {
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required]
        public string AssignedToUserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên công việc là bắt buộc")]
        [StringLength(200)]
        [Display(Name = "Tên công việc")]
        public string TaskName { get; set; } = string.Empty;

        [StringLength(2000)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Ngày hoàn thành là bắt buộc")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày hoàn thành (Deadline)")]
        public DateTime Deadline { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Mức độ ưu tiên")]
        public string Priority { get; set; } = "Trung bình"; // "Cao", "Trung bình", "Thấp"

        [Required]
        [StringLength(30)]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Chưa bắt đầu"; // "Chưa bắt đầu", "Đang thực hiện", "Hoàn thành", "Tạm dừng"

        [Range(0, 100)]
        [Display(Name = "Tiến độ (%)")]
        public int Progress { get; set; } = 0;

        [StringLength(500)]
        [Display(Name = "File đính kèm")]
        public string? AttachmentPath { get; set; }

        [StringLength(2000)]
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public Project Project { get; set; } = null!;
        public ApplicationUser AssignedToUser { get; set; } = null!;
    }
}
