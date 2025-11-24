using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using thuctapCN.Data;
using thuctapCN.Models;

namespace thuctapCN.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private readonly thuctapCNContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TaskController> _logger;
        private readonly IWebHostEnvironment _environment;

        public TaskController(
            thuctapCNContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<TaskController> logger,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _environment = environment;
        }

        // GET: Task/Index/{projectId} - Danh sách tasks của dự án (Manager/Admin)
        public async Task<IActionResult> Index(int? projectId)
        {
            if (projectId == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return NotFound();

            // Check if user is Manager of this project or is Admin
            if (!User.IsInRole("Admin"))
            {
                var isManager = await _context.ProjectMembers
                    .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == currentUser.Id && pm.Role == "Manager");

                if (!isManager)
                {
                    return Forbid();
                }
            }

            var tasks = await _context.TaskAssignments
                .Where(t => t.ProjectId == projectId)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Project)
                .Select(t => new TaskListViewModel
                {
                    Id = t.Id,
                    TaskName = t.TaskName,
                    Priority = t.Priority,
                    Status = t.Status,
                    Progress = t.Progress,
                    Deadline = t.Deadline,
                    AssignedToUserName = t.AssignedToUser.FullName ?? t.AssignedToUser.Email ?? "",
                    AssignedToEmployeeCode = t.AssignedToUser.EmployeeCode ?? "",
                    ProjectId = t.ProjectId,
                    ProjectName = t.Project.Name,
                    ProjectCode = t.Project.ProjectCode,
                    IsOverdue = t.Deadline < DateTime.Now && t.Status != "Hoàn thành"
                })
                .OrderBy(t => t.Status == "Hoàn thành" ? 1 : 0)
                .ThenBy(t => t.Deadline)
                .ToListAsync();

            ViewBag.ProjectId = projectId;
            ViewBag.ProjectName = project.Name;
            ViewBag.ProjectCode = project.ProjectCode;

            return View(tasks);
        }

        // GET: Task/MyTasks - Danh sách công việc của member
        public async Task<IActionResult> MyTasks()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var tasks = await _context.TaskAssignments
                .Where(t => t.AssignedToUserId == currentUser.Id)
                .Include(t => t.Project)
                .Include(t => t.AssignedToUser)
                .Select(t => new TaskListViewModel
                {
                    Id = t.Id,
                    TaskName = t.TaskName,
                    Priority = t.Priority,
                    Status = t.Status,
                    Progress = t.Progress,
                    Deadline = t.Deadline,
                    AssignedToUserName = t.AssignedToUser.FullName ?? t.AssignedToUser.Email ?? "",
                    AssignedToEmployeeCode = t.AssignedToUser.EmployeeCode ?? "",
                    ProjectId = t.ProjectId,
                    ProjectName = t.Project.Name,
                    ProjectCode = t.Project.ProjectCode,
                    IsOverdue = t.Deadline < DateTime.Now && t.Status != "Hoàn thành"
                })
                .OrderBy(t => t.Status == "Hoàn thành" ? 1 : 0)
                .ThenBy(t => t.Deadline)
                .ToListAsync();

            return View(tasks);
        }

        // GET: Task/Create/{projectId} - Form tạo task (Manager/Admin)
        public async Task<IActionResult> Create(int? projectId)
        {
            if (projectId == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return NotFound();

            // Check if user is Manager of this project or is Admin
            if (!User.IsInRole("Admin"))
            {
                var isManager = await _context.ProjectMembers
                    .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == currentUser.Id && pm.Role == "Manager");

                if (!isManager)
                {
                    return Forbid();
                }
            }

            // Get members in this project
            var memberIds = await _context.ProjectMembers
                .Where(pm => pm.ProjectId == projectId && pm.Role == "Member")
                .Select(pm => pm.UserId)
                .ToListAsync();

            var availableMembers = await _userManager.Users
                .Where(u => memberIds.Contains(u.Id))
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var model = new CreateTaskViewModel
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                ProjectCode = project.ProjectCode,
                AvailableMembers = availableMembers,
                ProjectStartDate = project.StartDate,
                ProjectEndDate = project.EndDate,
                Deadline = DateTime.Now.AddDays(7) > project.EndDate ? project.EndDate : DateTime.Now.AddDays(7)
            };

            return View(model);
        }

        // POST: Task/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTaskViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var project = await _context.Projects.FindAsync(model.ProjectId);
            if (project == null) return NotFound();

            // Check if user is Manager of this project or is Admin
            if (!User.IsInRole("Admin"))
            {
                var isManager = await _context.ProjectMembers
                    .AnyAsync(pm => pm.ProjectId == model.ProjectId && pm.UserId == currentUser.Id && pm.Role == "Manager");

                if (!isManager)
                {
                    return Forbid();
                }
            }

            // Validate deadline trong phạm vi dự án
            if (model.Deadline < project.StartDate || model.Deadline > project.EndDate)
            {
                ModelState.AddModelError("Deadline", "Ngày hoàn thành phải nằm trong thời gian dự án.");
            }

            // Validate deadline không sớm hơn ngày hiện tại
            if (model.Deadline.Date < DateTime.Now.Date)
            {
                ModelState.AddModelError("Deadline", "Ngày hoàn thành không thể sớm hơn ngày hiện tại.");
            }

            // Validate member được chọn thuộc dự án
            var isMemberInProject = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == model.ProjectId && pm.UserId == model.AssignedToUserId);

            if (!isMemberInProject)
            {
                ModelState.AddModelError("AssignedToUserId", "Thành viên được chọn không thuộc dự án này.");
            }

            if (!ModelState.IsValid)
            {
                // Reload available members
                var memberIds = await _context.ProjectMembers
                    .Where(pm => pm.ProjectId == model.ProjectId && pm.Role == "Member")
                    .Select(pm => pm.UserId)
                    .ToListAsync();

                model.AvailableMembers = await _userManager.Users
                    .Where(u => memberIds.Contains(u.Id))
                    .OrderBy(u => u.FullName)
                    .ToListAsync();

                model.ProjectStartDate = project.StartDate;
                model.ProjectEndDate = project.EndDate;

                return View(model);
            }

            try
            {
                var task = new TaskAssignment
                {
                    ProjectId = model.ProjectId,
                    AssignedToUserId = model.AssignedToUserId,
                    TaskName = model.TaskName,
                    Description = model.Description,
                    Deadline = model.Deadline,
                    Priority = model.Priority,
                    Status = "Chưa bắt đầu",
                    Progress = 0,
                    CreatedDate = DateTime.Now
                };

                _context.TaskAssignments.Add(task);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Task '{task.TaskName}' được tạo cho dự án {project.Name}");
                TempData["SuccessMessage"] = "Phân chia công việc thành công!";

                return RedirectToAction(nameof(Index), new { projectId = model.ProjectId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo task");
                ModelState.AddModelError(string.Empty, "Không thể lưu công việc. Vui lòng thử lại sau.");

                // Reload available members
                var memberIds = await _context.ProjectMembers
                    .Where(pm => pm.ProjectId == model.ProjectId && pm.Role == "Member")
                    .Select(pm => pm.UserId)
                    .ToListAsync();

                model.AvailableMembers = await _userManager.Users
                    .Where(u => memberIds.Contains(u.Id))
                    .OrderBy(u => u.FullName)
                    .ToListAsync();

                model.ProjectStartDate = project.StartDate;
                model.ProjectEndDate = project.EndDate;

                return View(model);
            }
        }

        // GET: Task/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var task = await _context.TaskAssignments
                .Include(t => t.Project)
                .Include(t => t.AssignedToUser)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return NotFound();

            // Check authorization
            var isManager = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == task.ProjectId && pm.UserId == currentUser.Id && pm.Role == "Manager");

            var isInProject = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == task.ProjectId && pm.UserId == currentUser.Id);

            var isAssignedUser = task.AssignedToUserId == currentUser.Id;
            var isAdmin = User.IsInRole("Admin");

            if (!isManager && !isInProject && !isAssignedUser && !isAdmin)
            {
                return Forbid();
            }

            var model = new TaskDetailsViewModel
            {
                Id = task.Id,
                TaskName = task.TaskName,
                Description = task.Description,
                Deadline = task.Deadline,
                Priority = task.Priority,
                Status = task.Status,
                Progress = task.Progress,
                Notes = task.Notes,
                AttachmentPath = task.AttachmentPath,
                CreatedDate = task.CreatedDate,
                UpdatedDate = task.UpdatedDate,
                ProjectId = task.ProjectId,
                ProjectName = task.Project.Name,
                ProjectCode = task.Project.ProjectCode,
                AssignedToUserId = task.AssignedToUserId,
                AssignedToUserName = task.AssignedToUser.FullName ?? task.AssignedToUser.Email ?? "",
                AssignedToEmployeeCode = task.AssignedToUser.EmployeeCode ?? "",
                IsOverdue = task.Deadline < DateTime.Now && task.Status != "Hoàn thành",
                CanEdit = isManager || isAdmin,
                CanUpdateStatus = isAssignedUser || isManager || isAdmin
            };

            // Load comments
            var comments = await _context.TaskComments
                .Where(c => c.TaskAssignmentId == id)
                .Include(c => c.User)
                .OrderBy(c => c.CreatedDate)
                .Select(c => new CommentViewModel
                {
                    Id = c.Id,
                    Content = c.Content,
                    UserId = c.UserId,
                    UserName = c.User.FullName ?? c.User.Email ?? "",
                    UserAvatar = c.User.AvatarPath,
                    CreatedDate = c.CreatedDate,
                    UpdatedDate = c.UpdatedDate,
                    CanEdit = c.UserId == currentUser.Id,
                    CanDelete = c.UserId == currentUser.Id || isManager || isAdmin
                })
                .ToListAsync();

            ViewBag.Comments = comments;
            ViewBag.CanComment = isAssignedUser || isInProject || isAdmin;

            return View(model);
        }

        // GET: Task/Edit/{id} (Manager/Admin)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var task = await _context.TaskAssignments
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return NotFound();

            // Check if user is Manager of this project or is Admin
            if (!User.IsInRole("Admin"))
            {
                var isManager = await _context.ProjectMembers
                    .AnyAsync(pm => pm.ProjectId == task.ProjectId && pm.UserId == currentUser.Id && pm.Role == "Manager");

                if (!isManager)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền cập nhật công việc này. Chỉ Manager hoặc Admin mới có quyền chỉnh sửa thông tin công việc.";
                    return RedirectToAction(nameof(Details), new { id });
                }
            }

            // Get members in this project
            var memberIds = await _context.ProjectMembers
                .Where(pm => pm.ProjectId == task.ProjectId && pm.Role == "Member")
                .Select(pm => pm.UserId)
                .ToListAsync();

            var availableMembers = await _userManager.Users
                .Where(u => memberIds.Contains(u.Id))
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var model = new EditTaskViewModel
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                ProjectName = task.Project.Name,
                ProjectCode = task.Project.ProjectCode,
                TaskName = task.TaskName,
                Description = task.Description,
                Deadline = task.Deadline,
                Priority = task.Priority,
                AssignedToUserId = task.AssignedToUserId,
                Status = task.Status,
                Progress = task.Progress,
                AvailableMembers = availableMembers,
                ProjectStartDate = task.Project.StartDate,
                ProjectEndDate = task.Project.EndDate
            };

            return View(model);
        }

        // POST: Task/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditTaskViewModel model)
        {
            if (id != model.Id) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var task = await _context.TaskAssignments
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return NotFound();

            // Check if user is Manager of this project or is Admin
            if (!User.IsInRole("Admin"))
            {
                var isManager = await _context.ProjectMembers
                    .AnyAsync(pm => pm.ProjectId == task.ProjectId && pm.UserId == currentUser.Id && pm.Role == "Manager");

                if (!isManager)
                {
                    return Forbid();
                }
            }

            // Validate deadline trong phạm vi dự án
            if (model.Deadline < task.Project.StartDate || model.Deadline > task.Project.EndDate)
            {
                ModelState.AddModelError("Deadline", "Ngày hoàn thành phải nằm trong thời gian dự án.");
            }

            // Validate member được chọn thuộc dự án
            var isMemberInProject = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == task.ProjectId && pm.UserId == model.AssignedToUserId);

            if (!isMemberInProject)
            {
                ModelState.AddModelError("AssignedToUserId", "Thành viên được chọn không thuộc dự án này.");
            }

            if (!ModelState.IsValid)
            {
                // Reload available members
                var memberIds = await _context.ProjectMembers
                    .Where(pm => pm.ProjectId == task.ProjectId && pm.Role == "Member")
                    .Select(pm => pm.UserId)
                    .ToListAsync();

                model.AvailableMembers = await _userManager.Users
                    .Where(u => memberIds.Contains(u.Id))
                    .OrderBy(u => u.FullName)
                    .ToListAsync();

                model.ProjectStartDate = task.Project.StartDate;
                model.ProjectEndDate = task.Project.EndDate;

                return View(model);
            }

            try
            {
                task.TaskName = model.TaskName;
                task.Description = model.Description;
                task.Deadline = model.Deadline;
                task.Priority = model.Priority;
                task.AssignedToUserId = model.AssignedToUserId;
                task.Status = model.Status;
                task.Progress = model.Progress;
                task.UpdatedDate = DateTime.Now;

                _context.Update(task);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Task '{task.TaskName}' được cập nhật");
                TempData["SuccessMessage"] = "Đã cập nhật công việc thành công!";

                return RedirectToAction(nameof(Index), new { projectId = task.ProjectId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật task");
                ModelState.AddModelError(string.Empty, "Không thể cập nhật công việc. Vui lòng thử lại sau.");

                // Reload available members
                var memberIds = await _context.ProjectMembers
                    .Where(pm => pm.ProjectId == task.ProjectId && pm.Role == "Member")
                    .Select(pm => pm.UserId)
                    .ToListAsync();

                model.AvailableMembers = await _userManager.Users
                    .Where(u => memberIds.Contains(u.Id))
                    .OrderBy(u => u.FullName)
                    .ToListAsync();

                model.ProjectStartDate = task.Project.StartDate;
                model.ProjectEndDate = task.Project.EndDate;

                return View(model);
            }
        }

        // POST: Task/Delete/{id} (Manager/Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var task = await _context.TaskAssignments.FindAsync(id);
            if (task == null) return NotFound();

            // Check if user is Manager of this project or is Admin
            if (!User.IsInRole("Admin"))
            {
                var isManager = await _context.ProjectMembers
                    .AnyAsync(pm => pm.ProjectId == task.ProjectId && pm.UserId == currentUser.Id && pm.Role == "Manager");

                if (!isManager)
                {
                    return Forbid();
                }
            }

            try
            {
                // Delete attachment file if exists
                if (!string.IsNullOrEmpty(task.AttachmentPath))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, task.AttachmentPath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.TaskAssignments.Remove(task);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Task '{task.TaskName}' đã bị xóa");
                TempData["SuccessMessage"] = "Đã xóa công việc thành công!";

                return RedirectToAction(nameof(Index), new { projectId = task.ProjectId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa task");
                TempData["ErrorMessage"] = "Không thể xóa công việc. Vui lòng thử lại sau.";
                return RedirectToAction(nameof(Details), new { id = task.Id });
            }
        }

        // GET: Task/UpdateStatus/{id} - Member cập nhật trạng thái
        public async Task<IActionResult> UpdateStatus(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var task = await _context.TaskAssignments
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return NotFound();

            // Check authorization: must be assigned user, manager, or admin
            var isManager = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == task.ProjectId && pm.UserId == currentUser.Id && pm.Role == "Manager");

            var isAssignedUser = task.AssignedToUserId == currentUser.Id;
            var isAdmin = User.IsInRole("Admin");

            if (!isAssignedUser && !isManager && !isAdmin)
            {
                return Forbid();
            }

            var model = new UpdateTaskStatusViewModel
            {
                Id = task.Id,
                TaskName = task.TaskName,
                ProjectName = task.Project.Name,
                Status = task.Status,
                Progress = task.Progress,
                CurrentProgress = task.Progress,
                Notes = task.Notes,
                CurrentAttachmentPath = task.AttachmentPath
            };

            return View(model);
        }

        // POST: Task/UpdateStatus/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, UpdateTaskStatusViewModel model)
        {
            if (id != model.Id) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var task = await _context.TaskAssignments
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return NotFound();

            // Check authorization
            var isManager = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == task.ProjectId && pm.UserId == currentUser.Id && pm.Role == "Manager");

            var isAssignedUser = task.AssignedToUserId == currentUser.Id;
            var isAdmin = User.IsInRole("Admin");

            if (!isAssignedUser && !isManager && !isAdmin)
            {
                return Forbid();
            }

            // Validate: Nếu status = "Hoàn thành" thì progress phải = 100%
            if (model.Status == "Hoàn thành" && model.Progress < 100)
            {
                ModelState.AddModelError("Progress", "Trạng thái \"Hoàn thành\" yêu cầu tiến độ 100%. Vui lòng kiểm tra lại.");
            }

            // Validate file upload
            if (model.AttachmentFile != null)
            {
                // Check file size (10MB max)
                if (model.AttachmentFile.Length > 10 * 1024 * 1024)
                {
                    ModelState.AddModelError("AttachmentFile", "Kích thước file không được vượt quá 10MB.");
                }

                // Check file extension
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".zip", ".rar", ".jpg", ".png", ".jpeg" };
                var fileExtension = Path.GetExtension(model.AttachmentFile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("AttachmentFile", "Định dạng file không được hỗ trợ. Vui lòng upload file: PDF, DOC, DOCX, XLS, XLSX, ZIP, RAR, JPG, PNG.");
                }
            }

            if (!ModelState.IsValid)
            {
                model.TaskName = task.TaskName;
                model.ProjectName = task.Project.Name;
                model.CurrentProgress = task.Progress;
                model.CurrentAttachmentPath = task.AttachmentPath;
                return View(model);
            }

            try
            {
                task.Status = model.Status;
                task.Progress = model.Progress;
                task.Notes = model.Notes;
                task.UpdatedDate = DateTime.Now;

                // Handle file upload
                if (model.AttachmentFile != null)
                {
                    // Delete old file if exists
                    if (!string.IsNullOrEmpty(task.AttachmentPath))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, task.AttachmentPath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Save new file
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "tasks");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = $"{task.Id}_{DateTime.Now:yyyyMMddHHmmss}_{model.AttachmentFile.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.AttachmentFile.CopyToAsync(fileStream);
                    }

                    task.AttachmentPath = $"/uploads/tasks/{uniqueFileName}";
                }

                _context.Update(task);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Task '{task.TaskName}' status updated to '{task.Status}' by {currentUser.Email}");
                TempData["SuccessMessage"] = "Cập nhật trạng thái công việc thành công!";

                return RedirectToAction(nameof(Details), new { id = task.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật task status");
                ModelState.AddModelError(string.Empty, "Không thể cập nhật trạng thái. Vui lòng thử lại sau.");
                
                model.TaskName = task.TaskName;
                model.ProjectName = task.Project.Name;
                model.CurrentProgress = task.Progress;
                model.CurrentAttachmentPath = task.AttachmentPath;
                
                return View(model);
            }
        }

        // POST: Task/AddComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int taskId, string content)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var task = await _context.TaskAssignments
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null) return NotFound();

            // Check authorization: assigned user, project member, or admin
            var isAssignedUser = task.AssignedToUserId == currentUser.Id;
            
            var isInProject = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == task.ProjectId && pm.UserId == currentUser.Id);

            var isAdmin = User.IsInRole("Admin");

            if (!isAssignedUser && !isInProject && !isAdmin)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền bình luận trong công việc này.";
                return RedirectToAction(nameof(Details), new { id = taskId });
            }

            // Validate content
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Nội dung bình luận không được để trống.";
                return RedirectToAction(nameof(Details), new { id = taskId });
            }

            if (content.Length > 2000)
            {
                TempData["ErrorMessage"] = "Nội dung bình luận không được vượt quá 2000 ký tự.";
                return RedirectToAction(nameof(Details), new { id = taskId });
            }

            try
            {
                var comment = new TaskComment
                {
                    TaskAssignmentId = taskId,
                    UserId = currentUser.Id,
                    Content = content.Trim(),
                    CreatedDate = DateTime.Now
                };

                _context.TaskComments.Add(comment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {currentUser.Email} commented on task {task.TaskName}");
                TempData["SuccessMessage"] = "Đã thêm bình luận thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment");
                TempData["ErrorMessage"] = "Không thể thêm bình luận. Vui lòng thử lại sau.";
            }

            return RedirectToAction(nameof(Details), new { id = taskId });
        }

        // POST: Task/EditComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditComment(int commentId, string content, int taskId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var comment = await _context.TaskComments.FindAsync(commentId);
            if (comment == null) return NotFound();

            // Check if user is the owner
            if (comment.UserId != currentUser.Id)
            {
                TempData["ErrorMessage"] = "Bạn chỉ có thể sửa bình luận của chính mình.";
                return RedirectToAction(nameof(Details), new { id = taskId });
            }

            // Validate content
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Nội dung bình luận không được để trống.";
                return RedirectToAction(nameof(Details), new { id = taskId });
            }

            if (content.Length > 2000)
            {
                TempData["ErrorMessage"] = "Nội dung bình luận không được vượt quá 2000 ký tự.";
                return RedirectToAction(nameof(Details), new { id = taskId });
            }

            try
            {
                comment.Content = content.Trim();
                comment.UpdatedDate = DateTime.Now;

                _context.Update(comment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {currentUser.Email} edited comment {commentId}");
                TempData["SuccessMessage"] = "Đã cập nhật bình luận thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing comment");
                TempData["ErrorMessage"] = "Không thể cập nhật bình luận. Vui lòng thử lại sau.";
            }

            return RedirectToAction(nameof(Details), new { id = taskId });
        }

        // POST: Task/DeleteComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId, int taskId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var comment = await _context.TaskComments
                .Include(c => c.TaskAssignment)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null) return NotFound();

            // Check authorization: owner, manager, or admin
            var isOwner = comment.UserId == currentUser.Id;

            var isManager = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == comment.TaskAssignment.ProjectId && 
                               pm.UserId == currentUser.Id && 
                               pm.Role == "Manager");

            var isAdmin = User.IsInRole("Admin");

            if (!isOwner && !isManager && !isAdmin)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xóa bình luận này.";
                return RedirectToAction(nameof(Details), new { id = taskId });
            }

            try
            {
                _context.TaskComments.Remove(comment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Comment {commentId} deleted by {currentUser.Email}");
                TempData["SuccessMessage"] = "Đã xóa bình luận thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment");
                TempData["ErrorMessage"] = "Không thể xóa bình luận. Vui lòng thử lại sau.";
            }

            return RedirectToAction(nameof(Details), new { id = taskId });
        }
    }
}
