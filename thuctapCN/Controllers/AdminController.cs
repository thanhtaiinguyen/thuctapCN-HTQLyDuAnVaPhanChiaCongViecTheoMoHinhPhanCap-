using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using thuctapCN.Data;
using thuctapCN.Models;

namespace thuctapCN.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly thuctapCNContext _context;

        public AdminController(
            UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager,
            thuctapCNContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    UserName = user.UserName,
                    EmployeeCode = user.EmployeeCode ?? string.Empty,
                    Roles = roles.ToList()
                });
            }

            return View(userViewModels);
        }
        public async Task<IActionResult> CreateUser()
        {
            var model = new CreateUserViewModel
            {
                AllRoles = await _roleManager.Roles.ToListAsync()
            };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra Mã nhân viên đã tồn tại chưa (chỉ kiểm tra nếu không rỗng)
                if (!string.IsNullOrWhiteSpace(model.EmployeeCode))
                {
                    var existingUser = await _userManager.Users
                        .FirstOrDefaultAsync(u => u.EmployeeCode == model.EmployeeCode);
                    
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("EmployeeCode", "Mã nhân viên đã tồn tại. Vui lòng sử dụng mã khác.");
                        model.AllRoles = await _roleManager.Roles.ToListAsync();
                        return View(model);
                    }
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true,
                    EmployeeCode = model.EmployeeCode
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Gán quyền cho người dùng
                    if (model.SelectedRoles != null && model.SelectedRoles.Any())
                    {
                        await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                    }
                    else
                    {
                        // Mặc định gán quyền "User" nếu không chọn quyền nào
                        await _userManager.AddToRoleAsync(user, "User");
                    }

                    TempData["SuccessMessage"] = $"Đã tạo user {model.Email} thành công!";
                    return RedirectToAction(nameof(Users));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            model.AllRoles = await _roleManager.Roles.ToListAsync();
            return View(model);
        }
        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName,
                EmployeeCode = user.EmployeeCode ?? string.Empty,
                SelectedRoles = userRoles.ToList(),
                AllRoles = await _roleManager.Roles.ToListAsync()
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                user.Email = model.Email;
                user.UserName = model.Email;
                // Mã nhân viên không được chỉnh sửa - giữ nguyên giá trị cũ
                // user.EmployeeCode = model.EmployeeCode; // Không cho phép sửa

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    var rolesToRemove = currentRoles.Except(model.SelectedRoles ?? new List<string>());
                    var rolesToAdd = (model.SelectedRoles ?? new List<string>()).Except(currentRoles);

                    if (rolesToRemove.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    }

                    if (rolesToAdd.Any())
                    {
                        await _userManager.AddToRolesAsync(user, rolesToAdd);
                    }
                    var finalRoles = await _userManager.GetRolesAsync(user);
                    if (!finalRoles.Any())
                    {
                        await _userManager.AddToRoleAsync(user, "User");
                    }

                    TempData["SuccessMessage"] = $"Đã cập nhật user {model.Email} thành công!";
                    return RedirectToAction(nameof(Users));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            model.AllRoles = await _roleManager.Roles.ToListAsync();
            return View(model);
        }
        public async Task<IActionResult> ChangePassword(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ChangePasswordViewModel
            {
                UserId = user.Id
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    return NotFound();
                }

                // Xóa mật khẩu cũ và đặt mật khẩu mới
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = $"Đã đổi mật khẩu cho user {user.Email} thành công!";
                    return RedirectToAction(nameof(Users));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Không cho phép xóa chính mình
            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["ErrorMessage"] = "Bạn không thể xóa chính tài khoản của mình!";
                return RedirectToAction(nameof(Users));
            }

            try
            {
                // Kiểm tra user có công việc được giao không
                var soCongViec = await _context.TaskAssignments.CountAsync(t => t.AssignedToUserId == id);
                
                if (soCongViec > 0)
                {
                    TempData["ErrorMessage"] = $"Không thể xóa user này vì đang có {soCongViec} công việc được giao. Vui lòng xóa hoặc chuyển công việc trước!";
                    return RedirectToAction(nameof(Users));
                }

                // Xóa các bình luận của user
                var binhLuan = await _context.TaskComments.Where(c => c.UserId == id).ToListAsync();
                if (binhLuan.Any())
                {
                    _context.TaskComments.RemoveRange(binhLuan);
                }

                // Xóa user khỏi các dự án
                var thanhVienDuAn = await _context.ProjectMembers.Where(pm => pm.UserId == id).ToListAsync();
                if (thanhVienDuAn.Any())
                {
                    _context.ProjectMembers.RemoveRange(thanhVienDuAn);
                }

                await _context.SaveChangesAsync();

                // Xóa user
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = $"Đã xóa user {user.Email} thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa user!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            }

            return RedirectToAction(nameof(Users));
        }
    }
}

