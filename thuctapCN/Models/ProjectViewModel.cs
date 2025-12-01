using System.ComponentModel.DataAnnotations;

namespace thuctapCN.Models
{
    public class CreateProjectViewModel
    {
        [Required(ErrorMessage = "Tên dự án là bắt buộc")]
        [StringLength(200)]
        [Display(Name = "Tên dự án")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày bắt đầu")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày kết thúc")]
        public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(1);

        [Display(Name = "Chọn Manager")]
        public List<string> SelectedManagerIds { get; set; } = new();

        // Để điền dữ liệu vào dropdown
        public List<ApplicationUser> AvailableUsers { get; set; } = new();
    }

    public class EditProjectViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Mã dự án")]
        public string ProjectCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên dự án là bắt buộc")]
        [StringLength(200)]
        [Display(Name = "Tên dự án")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày bắt đầu")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày kết thúc")]
        public DateTime EndDate { get; set; }
    }

    public class ProjectViewModel
    {
        public int Id { get; set; }
        public string ProjectCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ManagerCount { get; set; }
        public int MemberCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class AddMemberViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Vui lòng chọn thành viên")]
        [Display(Name = "Chọn thành viên")]
        public List<string> SelectedUserIds { get; set; } = new();
        
        public List<ApplicationUser> AvailableUsers { get; set; } = new();
        public List<ApplicationUser> CurrentMembers { get; set; } = new();
    }
}
