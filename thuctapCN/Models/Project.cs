using System.ComponentModel.DataAnnotations;

namespace thuctapCN.Models
{
    public class Project
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

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
    }
}
