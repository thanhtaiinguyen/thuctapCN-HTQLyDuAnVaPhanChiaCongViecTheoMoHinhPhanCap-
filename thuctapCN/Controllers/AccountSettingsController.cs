using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using thuctapCN.Models;

namespace thuctapCN.Controllers
{
    [Authorize]
    public class AccountSettingsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountSettingsController> _logger;

        public AccountSettingsController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountSettingsController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // GET: AccountSettings
        public IActionResult Index()
        {
            return View();
        }

        // GET: AccountSettings/ChangePassword
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: AccountSettings/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ProfileChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Thay đổi mật khẩu
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                // Đăng nhập lại để làm mới dấu bảo mật
                await _signInManager.RefreshSignInAsync(user);

                _logger.LogInformation($"User {user.Email} đã đổi mật khẩu thành công.");
                TempData["SuccessMessage"] = "Đã đổi mật khẩu thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Xử lý lỗi
            foreach (var error in result.Errors)
            {
                if (error.Code == "PasswordMismatch")
                {
                    ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }
    }
}
