using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using thuctapCN.Data;
using thuctapCN.Models;

namespace thuctapCN.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private readonly thuctapCNContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ProjectController> _logger;

        public ProjectController(
            thuctapCNContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<ProjectController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Dự án
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            List<ProjectViewModel> projects;

            if (User.IsInRole("Admin"))
            {
                // Admin xem tất cả dự án
                projects = await _context.Projects
                    .Include(p => p.ProjectMembers)
                    .Select(p => new ProjectViewModel
                    {
                        Id = p.Id,
                        ProjectCode = p.ProjectCode,
                        Name = p.Name,
                        Description = p.Description,
                        StartDate = p.StartDate,
                        EndDate = p.EndDate,
                        ManagerCount = p.ProjectMembers.Count(pm => pm.Role == "Manager"),
                        MemberCount = p.ProjectMembers.Count(pm => pm.Role == "Member"),
                        CreatedDate = p.CreatedDate
                    })
                    .OrderByDescending(p => p.CreatedDate)
                    .ToListAsync();
            }
            else
            {
                // Quản lý và Thành viên xem dự án được phân công
                projects = await _context.ProjectMembers
                    .Where(pm => pm.UserId == currentUser.Id)
                    .Include(pm => pm.Project)
                        .ThenInclude(p => p.ProjectMembers)
                    .Select(pm => new ProjectViewModel
                    {
                        Id = pm.Project.Id,
                        ProjectCode = pm.Project.ProjectCode,
                        Name = pm.Project.Name,
                        Description = pm.Project.Description,
                        StartDate = pm.Project.StartDate,
                        EndDate = pm.Project.EndDate,
                        ManagerCount = pm.Project.ProjectMembers.Count(m => m.Role == "Manager"),
                        MemberCount = pm.Project.ProjectMembers.Count(m => m.Role == "Member"),
                        CreatedDate = pm.Project.CreatedDate
                    })
                    .OrderByDescending(p => p.CreatedDate)
                    .ToListAsync();
            }

            return View(projects);
        }

        // GET: Dự án/Tạo mới (Chỉ Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var model = new CreateProjectViewModel
            {
                AvailableUsers = await _userManager.Users
                    .Where(u => u.Email != null)
                    .OrderBy(u => u.FullName)
                    .ToListAsync()
            };

            return View(model);
        }

        // POST: Dự án/Tạo mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateProjectViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableUsers = await _userManager.Users
                    .Where(u => u.Email != null)
                    .OrderBy(u => u.FullName)
                    .ToListAsync();
                return View(model);
            }

            // Xác thực ngày kết thúc >= ngày bắt đầu
            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError("EndDate", "Ngày kết thúc không được sớm hơn ngày bắt đầu");
                model.AvailableUsers = await _userManager.Users
                    .Where(u => u.Email != null)
                    .OrderBy(u => u.FullName)
                    .ToListAsync();
                return View(model);
            }

            // Xác thực tên dự án không trùng
            if (await _context.Projects.AnyAsync(p => p.Name == model.Name))
            {
                ModelState.AddModelError("Name", "Tên dự án đã trùng với một dự án khác");
                model.AvailableUsers = await _userManager.Users
                    .Where(u => u.Email != null)
                    .OrderBy(u => u.FullName)
                    .ToListAsync();
                return View(model);
            }

            try
            {
                // Tự động tạo Mã dự án
                var projectCode = await GenerateProjectCodeAsync();

                var project = new Project
                {
                    ProjectCode = projectCode,
                    Name = model.Name,
                    Description = model.Description,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    CreatedDate = DateTime.Now
                };

                _context.Projects.Add(project);
                await _context.SaveChangesAsync();

                // Thêm quản lý vào dự án
                if (model.SelectedManagerIds != null && model.SelectedManagerIds.Any())
                {
                    foreach (var userId in model.SelectedManagerIds)
                    {
                        var projectMember = new ProjectMember
                        {
                            ProjectId = project.Id,
                            UserId = userId,
                            Role = "Manager",
                            JoinedDate = DateTime.Now
                        };
                        _context.ProjectMembers.Add(projectMember);
                    }
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation($"Dự án {project.Name} (#{project.ProjectCode}) được tạo bởi Admin");
                TempData["SuccessMessage"] = "Tạo dự án thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo dự án");
                ModelState.AddModelError(string.Empty, "Không thể tạo dự án, vui lòng thử lại sau");
                model.AvailableUsers = await _userManager.Users
                    .Where(u => u.Email != null)
                    .OrderBy(u => u.FullName)
                    .ToListAsync();
                return View(model);
            }
        }

        // Phương thức hỗ trợ: Tạo Mã dự án duy nhất
        private async Task<string> GenerateProjectCodeAsync()
        {
            var lastProject = await _context.Projects
                .OrderByDescending(p => p.Id)
                .FirstOrDefaultAsync();

            if (lastProject == null)
            {
                return "PROJ-001";
            }

            // Trích xuất số từ mã cuối cùng (ví dụ: PROJ-001 -> 001)
            var lastNumber = int.Parse(lastProject.ProjectCode.Split('-')[1]);
            var newNumber = lastNumber + 1;

            return $"PROJ-{newNumber:D3}";
        }

        // GET: Dự án/Chỉnh sửa/5 (Chỉ Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();

            var model = new EditProjectViewModel
            {
                Id = project.Id,
                ProjectCode = project.ProjectCode,
                Name = project.Name,
                Description = project.Description,
                StartDate = project.StartDate,
                EndDate = project.EndDate
            };

            return View(model);
        }

        // POST: Dự án/Chỉnh sửa/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, EditProjectViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Xác thực ngày kết thúc >= ngày bắt đầu
            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError("EndDate", "Ngày kết thúc không được sớm hơn ngày bắt đầu");
                return View(model);
            }

            // Xác thực tên dự án không trùng với dự án khác
            if (await _context.Projects.AnyAsync(p => p.Name == model.Name && p.Id != model.Id))
            {
                ModelState.AddModelError("Name", "Tên dự án đã trùng với một dự án khác");
                return View(model);
            }

            try
            {
                var project = await _context.Projects.FindAsync(model.Id);
                if (project == null) return NotFound();

                // Mã dự án KHÔNG được thay đổi (bất biến)
                project.Name = model.Name;
                project.Description = model.Description;
                project.StartDate = model.StartDate;
                project.EndDate = model.EndDate;
                project.UpdatedDate = DateTime.Now;

                _context.Update(project);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Dự án {project.Name} (#{project.ProjectCode}) được cập nhật");
                TempData["SuccessMessage"] = "Đã cập nhật thông tin dự án!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật dự án");
                ModelState.AddModelError(string.Empty, "Không thể cập nhật dự án, vui lòng thử lại sau");
                return View(model);
            }
        }

        // GET: Dự án/Thêm thành viên/5 (Chỉ Quản lý, cho dự án của họ)
        public async Task<IActionResult> AddMember(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) return NotFound();

            // Kiểm tra xem người dùng có phải là Quản lý của dự án này hoặc là Admin không
            if (!User.IsInRole("Admin"))
            {
                var isManager = await _context.ProjectMembers
                    .AnyAsync(pm => pm.ProjectId == id && pm.UserId == currentUser.Id && pm.Role == "Manager");

                if (!isManager)
                {
                    return Forbid();
                }
            }

            // Lấy danh sách ID thành viên hiện tại
            var currentMemberIds = project.ProjectMembers.Select(pm => pm.UserId).ToList();

            var model = new AddMemberViewModel
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                AvailableUsers = await _userManager.Users
                    .Where(u => !currentMemberIds.Contains(u.Id) && u.Email != null)
                    .OrderBy(u => u.FullName)
                    .ToListAsync(),
                CurrentMembers = project.ProjectMembers
                    .Select(pm => pm.User)
                    .OrderBy(u => u.FullName)
                    .ToList()
            };

            return View(model);
        }

        // POST: Dự án/Thêm thành viên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(AddMemberViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var project = await _context.Projects.FindAsync(model.ProjectId);
            if (project == null) return NotFound();

            // Kiểm tra xem người dùng có phải là Quản lý của dự án này hoặc là Admin không
            if (!User.IsInRole("Admin"))
            {
                var isManager = await _context.ProjectMembers
                    .AnyAsync(pm => pm.ProjectId == model.ProjectId && pm.UserId == currentUser.Id && pm.Role == "Manager");

                if (!isManager)
                {
                    return Forbid();
                }
            }

            if (!ModelState.IsValid || model.SelectedUserIds == null || !model.SelectedUserIds.Any())
            {
                ModelState.AddModelError(string.Empty, "Vui lòng chọn ít nhất một thành viên");
                
                // Tải lại danh sách người dùng khả dụng
                var currentMemberIds = await _context.ProjectMembers
                    .Where(pm => pm.ProjectId == model.ProjectId)
                    .Select(pm => pm.UserId)
                    .ToListAsync();

                model.AvailableUsers = await _userManager.Users
                    .Where(u => !currentMemberIds.Contains(u.Id) && u.Email != null)
                    .OrderBy(u => u.FullName)
                    .ToListAsync();

                return View(model);
            }

            try
            {
                foreach (var userId in model.SelectedUserIds)
                {
                    // Kiểm tra xem người dùng đã tồn tại trong dự án chưa
                    if (!await _context.ProjectMembers.AnyAsync(pm => pm.ProjectId == model.ProjectId && pm.UserId == userId))
                    {
                        var projectMember = new ProjectMember
                        {
                            ProjectId = model.ProjectId,
                            UserId = userId,
                            Role = "Member",
                            JoinedDate = DateTime.Now
                        };
                        _context.ProjectMembers.Add(projectMember);
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Thêm {model.SelectedUserIds.Count} thành viên vào dự án {project.Name}");
                TempData["SuccessMessage"] = "Đã thêm thành viên vào dự án!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm thành viên");
                ModelState.AddModelError(string.Empty, "Không thể thêm thành viên, vui lòng thử lại sau");
                
                // Tải lại danh sách người dùng khả dụng
                var currentMemberIds = await _context.ProjectMembers
                    .Where(pm => pm.ProjectId == model.ProjectId)
                    .Select(pm => pm.UserId)
                    .ToListAsync();

                model.AvailableUsers = await _userManager.Users
                    .Where(u => !currentMemberIds.Contains(u.Id) && u.Email != null)
                    .OrderBy(u => u.FullName)
                    .ToListAsync();

                return View(model);
            }
        }
        // GET: Dự án/Xóa/5 (Chỉ Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null) return NotFound();

            return View(project);
        }

        // POST: Dự án/Xóa/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Dự án {project.Name} (#{project.ProjectCode}) đã bị xóa bởi Admin");
                TempData["SuccessMessage"] = "Đã xóa dự án thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
        // POST: Dự án/Xóa thành viên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int projectId, string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return NotFound();

            // Kiểm tra quyền hạn
            bool isAdmin = User.IsInRole("Admin");
            bool isManager = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == currentUser.Id && pm.Role == "Manager");

            if (!isAdmin && !isManager)
            {
                return Forbid();
            }

            var memberToRemove = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

            if (memberToRemove == null)
            {
                TempData["ErrorMessage"] = "Thành viên không tồn tại trong dự án này.";
                return RedirectToAction(nameof(AddMember), new { id = projectId });
            }

            // Quản lý không thể xóa các Quản lý khác
            if (!isAdmin && memberToRemove.Role == "Manager")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xóa Quản lý dự án.";
                return RedirectToAction(nameof(AddMember), new { id = projectId });
            }

            // Không thể tự xóa mình thông qua hành động này (sử dụng Rời khỏi nếu đã được triển khai, hoặc chỉ cho phép với cảnh báo?)
            // Thông thường việc tự xóa mình là "Rời khỏi". Hãy ngăn chặn việc tự xóa mình ở đây để tránh bị khóa quyền truy cập nếu bạn là quản lý duy nhất.
            if (userId == currentUser.Id)
            {
                TempData["ErrorMessage"] = "Bạn không thể tự xóa mình khỏi dự án tại đây.";
                return RedirectToAction(nameof(AddMember), new { id = projectId });
            }

            try
            {
                _context.ProjectMembers.Remove(memberToRemove);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đã xóa thành viên khỏi dự án.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa thành viên khỏi dự án");
                TempData["ErrorMessage"] = "Không thể xóa thành viên. Vui lòng thử lại sau.";
            }

            return RedirectToAction(nameof(AddMember), new { id = projectId });
        }

        // ═══════════════════════════════════════════════════════════════
        // BÌNH LUẬN DỰ ÁN
        // ═══════════════════════════════════════════════════════════════

        // GET: Project/Comments/5 - Xem bình luận của dự án
        public async Task<IActionResult> Comments(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();

            // Kiểm tra quyền: Admin hoặc thành viên dự án
            bool isAdmin = User.IsInRole("Admin");
            bool isMember = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == id && pm.UserId == currentUser.Id);
            bool isManager = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == id && pm.UserId == currentUser.Id && pm.Role == "Manager");

            if (!isAdmin && !isMember)
            {
                return Forbid();
            }

            // Lấy danh sách bình luận
            var comments = await _context.ProjectComments
                .Where(c => c.ProjectId == id)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedDate)
                .Select(c => new ProjectCommentViewModel
                {
                    Id = c.Id,
                    ProjectId = c.ProjectId,
                    ProjectName = project.Name,
                    UserId = c.UserId,
                    UserName = c.User.FullName ?? c.User.Email ?? "",
                    UserAvatar = c.User.AvatarPath,
                    Content = c.Content,
                    CreatedDate = c.CreatedDate,
                    UpdatedDate = c.UpdatedDate,
                    CanEdit = c.UserId == currentUser.Id,
                    CanDelete = c.UserId == currentUser.Id || isManager || isAdmin
                })
                .ToListAsync();

            ViewBag.ProjectId = id;
            ViewBag.ProjectName = project.Name;
            ViewBag.ProjectCode = project.ProjectCode;
            ViewBag.CanComment = isMember || isAdmin;

            return View(comments);
        }

        // POST: Project/AddComment - Thêm bình luận
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int projectId, string content)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return NotFound();

            // Kiểm tra quyền: Admin hoặc thành viên dự án
            bool isAdmin = User.IsInRole("Admin");
            bool isMember = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == currentUser.Id);

            if (!isAdmin && !isMember)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền bình luận trong dự án này!";
                return RedirectToAction("Comments", new { id = projectId });
            }

            // Validate content
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Nội dung bình luận không được để trống!";
                return RedirectToAction("Comments", new { id = projectId });
            }

            if (content.Length > 2000)
            {
                TempData["ErrorMessage"] = "Nội dung bình luận không được vượt quá 2000 ký tự!";
                return RedirectToAction("Comments", new { id = projectId });
            }

            try
            {
                var comment = new ProjectComment
                {
                    ProjectId = projectId,
                    UserId = currentUser.Id,
                    Content = content.Trim(),
                    CreatedDate = DateTime.Now
                };

                _context.ProjectComments.Add(comment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {currentUser.Email} đã thêm bình luận vào dự án {project.Name}");
                TempData["SuccessMessage"] = "Đã thêm bình luận thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm bình luận");
                TempData["ErrorMessage"] = "Có lỗi xảy ra, vui lòng thử lại!";
            }

            return RedirectToAction("Comments", new { id = projectId });
        }

        // POST: Project/EditComment - Sửa bình luận
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditComment(int commentId, string content)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var comment = await _context.ProjectComments
                .Include(c => c.Project)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null) return NotFound();

            // Chỉ người viết mới được sửa
            if (comment.UserId != currentUser.Id)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền sửa bình luận này!";
                return RedirectToAction("Comments", new { id = comment.ProjectId });
            }

            // Validate content
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Nội dung bình luận không được để trống!";
                return RedirectToAction("Comments", new { id = comment.ProjectId });
            }

            if (content.Length > 2000)
            {
                TempData["ErrorMessage"] = "Nội dung bình luận không được vượt quá 2000 ký tự!";
                return RedirectToAction("Comments", new { id = comment.ProjectId });
            }

            try
            {
                comment.Content = content.Trim();
                comment.UpdatedDate = DateTime.Now;

                _context.Update(comment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {currentUser.Email} đã sửa bình luận trong dự án {comment.Project.Name}");
                TempData["SuccessMessage"] = "Đã cập nhật bình luận thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi sửa bình luận");
                TempData["ErrorMessage"] = "Có lỗi xảy ra, vui lòng thử lại!";
            }

            return RedirectToAction("Comments", new { id = comment.ProjectId });
        }

        // POST: Project/DeleteComment - Xóa bình luận
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId, bool fromAllComments = false)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var comment = await _context.ProjectComments
                .Include(c => c.Project)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null) return NotFound();

            // Kiểm tra quyền: Người viết, Manager, hoặc Admin
            bool isAdmin = User.IsInRole("Admin");
            bool isManager = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == comment.ProjectId && pm.UserId == currentUser.Id && pm.Role == "Manager");
            bool isOwner = comment.UserId == currentUser.Id;

            if (!isOwner && !isManager && !isAdmin)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xóa bình luận này!";
                if (fromAllComments)
                    return RedirectToAction("AllComments");
                return RedirectToAction("Comments", new { id = comment.ProjectId });
            }

            var projectId = comment.ProjectId;

            try
            {
                _context.ProjectComments.Remove(comment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {currentUser.Email} đã xóa bình luận trong dự án");
                TempData["SuccessMessage"] = "Đã xóa bình luận thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa bình luận");
                TempData["ErrorMessage"] = "Có lỗi xảy ra, vui lòng thử lại!";
            }

            if (fromAllComments)
                return RedirectToAction("AllComments");
            return RedirectToAction("Comments", new { id = projectId });
        }

        // GET: Project/AllComments - Admin xem tất cả bình luận
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllComments()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var comments = await _context.ProjectComments
                .Include(c => c.User)
                .Include(c => c.Project)
                .OrderByDescending(c => c.CreatedDate)
                .Select(c => new ProjectCommentViewModel
                {
                    Id = c.Id,
                    ProjectId = c.ProjectId,
                    ProjectName = c.Project.Name,
                    UserId = c.UserId,
                    UserName = c.User.FullName ?? c.User.Email ?? "",
                    UserAvatar = c.User.AvatarPath,
                    Content = c.Content,
                    CreatedDate = c.CreatedDate,
                    UpdatedDate = c.UpdatedDate,
                    CanEdit = false, // Admin không sửa bình luận người khác
                    CanDelete = true  // Admin có thể xóa tất cả
                })
                .ToListAsync();

            return View(comments);
        }
    }
}
