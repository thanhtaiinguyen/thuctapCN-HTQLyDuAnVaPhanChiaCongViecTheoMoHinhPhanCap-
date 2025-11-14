using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace thuctapCN.Models
{
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Mã nhân viên")]
        public string? EmployeeCode { get; set; }

        [Display(Name = "Tên người dùng")]
        public string? UserName { get; set; }

        [Display(Name = "Roles")]
        public List<string> Roles { get; set; } = new List<string>();

        [Display(Name = "Tất cả Roles")]
        public List<IdentityRole> AllRoles { get; set; } = new List<IdentityRole>();
    }

    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã nhân viên là bắt buộc")]
        [StringLength(50, ErrorMessage = "Mã nhân viên không được vượt quá 50 ký tự")]
        [Display(Name = "Mã nhân viên")]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu và mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Phân quyền")]
        public List<string> SelectedRoles { get; set; } = new List<string>();

        [Display(Name = "Tất cả Roles")]
        public List<IdentityRole> AllRoles { get; set; } = new List<IdentityRole>();
    }

    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Tên người dùng")]
        public string? UserName { get; set; }

        [Display(Name = "Mã nhân viên")]
        public string EmployeeCode { get; set; } = string.Empty;

        [Display(Name = "Phân quyền")]
        public List<string> SelectedRoles { get; set; } = new List<string>();

        [Display(Name = "Tất cả Roles")]
        public List<IdentityRole> AllRoles { get; set; } = new List<IdentityRole>();
    }

    public class ChangePasswordViewModel
    {
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu và mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

