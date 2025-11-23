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

        // GET: Project
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
                // Manager và Member xem dự án được assign
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

        // GET: Project/Create (Admin only)
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

        // POST: Project/Create
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

            // Validate ngày kết thúc >= ngày bắt đầu
            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError("EndDate", "Ngày kết thúc không được sớm hơn ngày bắt đầu");
                model.AvailableUsers = await _userManager.Users
                    .Where(u => u.Email != null)
                    .OrderBy(u => u.FullName)
                    .ToListAsync();
                return View(model);
            }

            // Validate tên dự án không trùng
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
                // Generate ProjectCode tự động
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

                // Thêm managers vào dự án
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

        // Helper method: Generate unique ProjectCode
        private async Task<string> GenerateProjectCodeAsync()
        {
            var lastProject = await _context.Projects
                .OrderByDescending(p => p.Id)
                .FirstOrDefaultAsync();

            if (lastProject == null)
            {
                return "PROJ-001";
            }

            // Extract number from last code (e.g., PROJ-001 -> 001)
            var lastNumber = int.Parse(lastProject.ProjectCode.Split('-')[1]);
            var newNumber = lastNumber + 1;

            return $"PROJ-{newNumber:D3}";
        }

        // GET: Project/Edit/5 (Admin only)
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

        // POST: Project/Edit/5
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

            // Validate ngày kết thúc >= ngày bắt đầu
            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError("EndDate", "Ngày kết thúc không được sớm hơn ngày bắt đầu");
                return View(model);
            }

            // Validate tên dự án không trùng với dự án khác
            if (await _context.Projects.AnyAsync(p => p.Name == model.Name && p.Id != model.Id))
            {
                ModelState.AddModelError("Name", "Tên dự án đã trùng với một dự án khác");
                return View(model);
            }

            try
            {
                var project = await _context.Projects.FindAsync(model.Id);
                if (project == null) return NotFound();

                // ProjectCode KHÔNG được thay đổi (immutable)
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

        // GET: Project/AddMember/5 (Manager only, for their projects)
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

            // Check if user is Manager of this project or is Admin
            if (!User.IsInRole("Admin"))
            {
                var isManager = await _context.ProjectMembers
                    .AnyAsync(pm => pm.ProjectId == id && pm.UserId == currentUser.Id && pm.Role == "Manager");

                if (!isManager)
                {
                    return Forbid();
                }
            }

            // Get current member IDs
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

        // POST: Project/AddMember
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(AddMemberViewModel model)
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

            if (!ModelState.IsValid || model.SelectedUserIds == null || !model.SelectedUserIds.Any())
            {
                ModelState.AddModelError(string.Empty, "Vui lòng chọn ít nhất một thành viên");
                
                // Reload available users
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
                    // Check if user already exists in project
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
                
                // Reload available users
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
    }
}
