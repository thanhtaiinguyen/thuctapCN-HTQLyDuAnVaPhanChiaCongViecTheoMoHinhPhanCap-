using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace thuctapCN.Models
{
    public class CreateTaskViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectCode { get; set; } = string.Empty;

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
        public DateTime Deadline { get; set; } = DateTime.Now.AddDays(7);

        [Required(ErrorMessage = "Mức độ ưu tiên là bắt buộc")]
        [Display(Name = "Mức độ ưu tiên")]
        public string Priority { get; set; } = "Trung bình";

        [Required(ErrorMessage = "Vui lòng chọn thành viên để giao việc")]
        [Display(Name = "Thành viên được giao")]
        public string AssignedToUserId { get; set; } = string.Empty;

        // Để điền dữ liệu vào dropdown
        public List<ApplicationUser> AvailableMembers { get; set; } = new();
        public DateTime ProjectStartDate { get; set; }
        public DateTime ProjectEndDate { get; set; }
    }

    public class EditTaskViewModel
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectCode { get; set; } = string.Empty;

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

        [Required(ErrorMessage = "Mức độ ưu tiên là bắt buộc")]
        [Display(Name = "Mức độ ưu tiên")]
        public string Priority { get; set; } = "Trung bình";

        [Required(ErrorMessage = "Vui lòng chọn thành viên để giao việc")]
        [Display(Name = "Thành viên được giao")]
        public string AssignedToUserId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Chưa bắt đầu";

        [Range(0, 100)]
        [Display(Name = "Tiến độ (%)")]
        public int Progress { get; set; } = 0;

        // Để điền dữ liệu vào dropdown
        public List<ApplicationUser> AvailableMembers { get; set; } = new();
        public DateTime ProjectStartDate { get; set; }
        public DateTime ProjectEndDate { get; set; }
    }

    public class TaskListViewModel
    {
        public int Id { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Progress { get; set; }
        public DateTime Deadline { get; set; }
        public string AssignedToUserName { get; set; } = string.Empty;
        public string AssignedToEmployeeCode { get; set; } = string.Empty;
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectCode { get; set; } = string.Empty;
        public bool IsOverdue { get; set; }
    }

    public class UpdateTaskStatusViewModel
    {
        public int Id { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public int CurrentProgress { get; set; }

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "Tiến độ phải từ 0 đến 100")]
        [Display(Name = "Tiến độ (%)")]
        public int Progress { get; set; }

        [StringLength(2000)]
        [Display(Name = "Ghi chú/Báo cáo")]
        public string? Notes { get; set; }

        [Display(Name = "File đính kèm")]
        public IFormFile? AttachmentFile { get; set; }

        public string? CurrentAttachmentPath { get; set; }
    }

    public class TaskDetailsViewModel
    {
        public int Id { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime Deadline { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Progress { get; set; }
        public string? Notes { get; set; }
        public string? AttachmentPath { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Thông tin dự án
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectCode { get; set; } = string.Empty;

        // Thông tin người được giao
        public string AssignedToUserId { get; set; } = string.Empty;
        public string AssignedToUserName { get; set; } = string.Empty;
        public string AssignedToEmployeeCode { get; set; } = string.Empty;

        public bool IsOverdue { get; set; }
        public bool CanEdit { get; set; }
        public bool CanUpdateStatus { get; set; }
    }
}
