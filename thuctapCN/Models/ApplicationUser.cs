using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace thuctapCN.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "Họ và tên")]
        [StringLength(100)]
        public string? FullName { get; set; }

        [Display(Name = "Mã nhân viên")]
        [Required(ErrorMessage = "Mã nhân viên là bắt buộc")]
        [StringLength(50)]
        public string? EmployeeCode { get; set; }

        [Display(Name = "Số điện thoại")]
        [StringLength(20)]
        public override string? PhoneNumber { get; set; }

        [Display(Name = "Địa chỉ")]
        [StringLength(500)]
        public string? Address { get; set; }

        [Display(Name = "Ảnh đại diện")]
        [StringLength(500)]
        public string? AvatarPath { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Giới tính")]
        [StringLength(10)]
        public string? Gender { get; set; }

        [Display(Name = "Phòng ban")]
        [StringLength(100)]
        public string? Department { get; set; }

        [Display(Name = "Chức vụ")]
        [StringLength(100)]
        public string? Position { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedDate { get; set; }
    }
}

