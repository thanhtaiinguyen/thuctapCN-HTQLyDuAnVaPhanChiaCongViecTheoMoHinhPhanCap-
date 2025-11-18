using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using thuctapCN.Models;

namespace thuctapCN.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment,
            ILogger<ProfileController> logger)
        {
            _userManager = userManager;
            _environment = environment;
            _logger = logger;
        }

        // GET: Profile
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                EmployeeCode = user.EmployeeCode ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                AvatarPath = user.AvatarPath,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Department = user.Department,
                Position = user.Position,
                CreatedDate = user.CreatedDate,
                UpdatedDate = user.UpdatedDate
            };

            return View(model);
        }

        // GET: Profile/Edit
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                EmployeeCode = user.EmployeeCode ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                AvatarPath = user.AvatarPath,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Department = user.Department,
                Position = user.Position,
                CreatedDate = user.CreatedDate,
                UpdatedDate = user.UpdatedDate
            };

            return View(model);
        }

        // POST: Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileViewModel model, IFormFile? avatarFile)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                // Kiểm tra user chỉ được sửa thông tin của chính mình
                if (user.Id != model.Id)
                {
                    return Forbid();
                }

                // Xử lý upload ảnh đại diện
                if (avatarFile != null && avatarFile.Length > 0)
                {
                    // Kiểm tra định dạng file
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("AvatarPath", "Chỉ chấp nhận file ảnh: .jpg, .jpeg, .png, .gif");
                        return View(model);
                    }

                    // Kiểm tra kích thước file (tối đa 5MB)
                    if (avatarFile.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("AvatarPath", "Kích thước file không được vượt quá 5MB");
                        return View(model);
                    }

                    // Tạo thư mục uploads nếu chưa có
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
                    try
                    {
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Không thể tạo thư mục uploads");
                        ModelState.AddModelError("AvatarPath", "Không thể tạo thư mục lưu ảnh. Vui lòng liên hệ quản trị viên.");
                        return View(model);
                    }

                    // Tạo tên file unique
                    var fileName = $"{user.Id}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Xóa ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(user.AvatarPath))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, user.AvatarPath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Lưu file mới
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await avatarFile.CopyToAsync(stream);
                    }

                    // Lưu đường dẫn ảnh (relative path)
                    user.AvatarPath = $"/uploads/avatars/{fileName}";
                }

                // Cập nhật thông tin user (không cho phép sửa EmployeeCode)
                user.Email = model.Email;
                user.UserName = model.Email;
                user.FullName = model.FullName;
                // EmployeeCode không được chỉnh sửa - giữ nguyên giá trị cũ
                // user.EmployeeCode = model.EmployeeCode; // Không cho phép sửa
                user.PhoneNumber = model.PhoneNumber;
                user.Address = model.Address;
                user.DateOfBirth = model.DateOfBirth;
                user.Gender = model.Gender;
                user.Department = model.Department;
                user.Position = model.Position;
                user.UpdatedDate = DateTime.Now;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Đã cập nhật thông tin thành công!";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }
    }
}

