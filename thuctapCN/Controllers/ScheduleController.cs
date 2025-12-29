using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using thuctapCN.Data;
using thuctapCN.Models;

namespace thuctapCN.Controllers
{
    [Authorize]
    public class ScheduleController : Controller
    {
        private readonly thuctapCNContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ScheduleController> _logger;

        public ScheduleController(
            thuctapCNContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<ScheduleController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }
        // GET: Schedule - Xem lịch của mình
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            ViewBag.IsAdmin = User.IsInRole("Admin");
            ViewBag.IsManager = await _context.ProjectMembers
                .AnyAsync(pm => pm.UserId == currentUser.Id && pm.Role == "Manager");

            return View();
        }

        // GET: Schedule/GetEvents - Lấy sự kiện cho calendar (JSON)
        public async Task<IActionResult> GetEvents(DateTime start, DateTime end)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Json(new List<CalendarEventViewModel>());

            var isAdmin = User.IsInRole("Admin");

            IQueryable<WorkSchedule> query;

            if (isAdmin)
            {
                // Admin xem tất cả
                query = _context.WorkSchedules;
            }
            else
            {
                // User chỉ xem lịch của mình
                query = _context.WorkSchedules.Where(s => s.UserId == currentUser.Id);
            }

            var events = await query
                .Where(s => s.StartDate >= start && s.EndDate <= end)
                .Include(s => s.User)
                .Select(s => new CalendarEventViewModel
                {
                    id = s.Id,
                    title = isAdmin ? $"{s.User.FullName}: {s.Title}" : s.Title,
                    start = s.StartDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = s.EndDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    allDay = s.IsAllDay,
                    color = s.Color,
                    url = Url.Action("Details", new { id = s.Id }) ?? "",
                    description = s.Description ?? ""
                })
                .ToListAsync();

            return Json(events);
        }

        // GET: Schedule/Create
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var model = new CreateScheduleViewModel
            {
                StartDate = DateTime.Today.AddHours(9),
                EndDate = DateTime.Today.AddHours(18)
            };

            // Load người dùng có thể xếp lịch
            if (User.IsInRole("Admin"))
            {
                model.AvailableUsers = await _userManager.Users.OrderBy(u => u.FullName).ToListAsync();
                model.AvailableProjects = await _context.Projects.OrderBy(p => p.Name).ToListAsync();
            }
            else
            {
                // Manager chỉ xếp lịch cho thành viên trong dự án mình quản lý
                var managedProjectIds = await _context.ProjectMembers
                    .Where(pm => pm.UserId == currentUser.Id && pm.Role == "Manager")
                    .Select(pm => pm.ProjectId)
                    .ToListAsync();

                var memberIds = await _context.ProjectMembers
                    .Where(pm => managedProjectIds.Contains(pm.ProjectId))
                    .Select(pm => pm.UserId)
                    .Distinct()
                    .ToListAsync();

                model.AvailableUsers = await _userManager.Users
                    .Where(u => memberIds.Contains(u.Id))
                    .OrderBy(u => u.FullName)
                    .ToListAsync();

                model.AvailableProjects = await _context.Projects
                    .Where(p => managedProjectIds.Contains(p.Id))
                    .OrderBy(p => p.Name)
                    .ToListAsync();
            }

            return View(model);
        }

        // POST: Schedule/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create(CreateScheduleViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            // Validate ngày
            if (model.EndDate <= model.StartDate)
            {
                ModelState.AddModelError("EndDate", "Ngày kết thúc phải sau ngày bắt đầu!");
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdownData(model, currentUser);
                return View(model);
            }

            try
            {
                var schedule = new WorkSchedule
                {
                    UserId = model.UserId,
                    ProjectId = model.ProjectId,
                    Title = model.Title,
                    Description = model.Description,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    IsAllDay = model.IsAllDay,
                    Color = model.Color,
                    ScheduleType = model.ScheduleType,
                    Location = model.Location,
                    CreatedByUserId = currentUser.Id,
                    CreatedDate = DateTime.Now
                };

                _context.WorkSchedules.Add(schedule);
                await _context.SaveChangesAsync();

                // Gửi thông báo cho người được xếp lịch
                if (model.UserId != currentUser.Id)
                {
                    var notification = new Notification
                    {
                        UserId = model.UserId,
                        Title = "Lịch làm việc mới",
                        Content = $"Bạn có lịch làm việc mới: {model.Title} vào ngày {model.StartDate:dd/MM/yyyy}",
                        Type = "Info",
                        RelatedUrl = Url.Action("Details", new { id = schedule.Id }),
                        CreatedDate = DateTime.Now
                    };
                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation($"User {currentUser.Email} đã tạo lịch làm việc mới");
                TempData["SuccessMessage"] = "Đã tạo lịch làm việc thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo lịch làm việc");
                ModelState.AddModelError("", "Có lỗi xảy ra, vui lòng thử lại!");
                await LoadDropdownData(model, currentUser);
                return View(model);
            }
        }

        // GET: Schedule/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var schedule = await _context.WorkSchedules
                .Include(s => s.User)
                .Include(s => s.CreatedByUser)
                .Include(s => s.Project)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null) return NotFound();

            // Kiểm tra quyền
            bool isAdmin = User.IsInRole("Admin");
            bool isOwner = schedule.UserId == currentUser.Id;
            bool isCreator = schedule.CreatedByUserId == currentUser.Id;

            if (!isAdmin && !isOwner && !isCreator)
            {
                return Forbid();
            }

            var model = new ScheduleViewModel
            {
                Id = schedule.Id,
                UserId = schedule.UserId,
                UserName = schedule.User.FullName ?? schedule.User.Email ?? "",
                UserAvatar = schedule.User.AvatarPath,
                ProjectId = schedule.ProjectId,
                ProjectName = schedule.Project?.Name,
                Title = schedule.Title,
                Description = schedule.Description,
                StartDate = schedule.StartDate,
                EndDate = schedule.EndDate,
                IsAllDay = schedule.IsAllDay,
                Color = schedule.Color,
                ScheduleType = schedule.ScheduleType,
                Location = schedule.Location,
                CreatedByUserName = schedule.CreatedByUser.FullName ?? schedule.CreatedByUser.Email ?? "",
                CreatedDate = schedule.CreatedDate,
                CanEdit = isAdmin || isCreator,
                CanDelete = isAdmin || isCreator
            };

            return View(model);
        }

        // GET: Schedule/Edit/{id}
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var schedule = await _context.WorkSchedules.FindAsync(id);
            if (schedule == null) return NotFound();

            // Kiểm tra quyền
            if (!User.IsInRole("Admin") && schedule.CreatedByUserId != currentUser.Id)
            {
                return Forbid();
            }

            var model = new CreateScheduleViewModel
            {
                Id = schedule.Id,
                UserId = schedule.UserId,
                ProjectId = schedule.ProjectId,
                Title = schedule.Title,
                Description = schedule.Description,
                StartDate = schedule.StartDate,
                EndDate = schedule.EndDate,
                IsAllDay = schedule.IsAllDay,
                Color = schedule.Color,
                ScheduleType = schedule.ScheduleType,
                Location = schedule.Location
            };

            await LoadDropdownData(model, currentUser);
            return View(model);
        }

        // POST: Schedule/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id, CreateScheduleViewModel model)
        {
            if (id != model.Id) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var schedule = await _context.WorkSchedules.FindAsync(id);
            if (schedule == null) return NotFound();

            if (!User.IsInRole("Admin") && schedule.CreatedByUserId != currentUser.Id)
            {
                return Forbid();
            }

            if (model.EndDate <= model.StartDate)
            {
                ModelState.AddModelError("EndDate", "Ngày kết thúc phải sau ngày bắt đầu!");
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdownData(model, currentUser);
                return View(model);
            }

            try
            {
                schedule.UserId = model.UserId;
                schedule.ProjectId = model.ProjectId;
                schedule.Title = model.Title;
                schedule.Description = model.Description;
                schedule.StartDate = model.StartDate;
                schedule.EndDate = model.EndDate;
                schedule.IsAllDay = model.IsAllDay;
                schedule.Color = model.Color;
                schedule.ScheduleType = model.ScheduleType;
                schedule.Location = model.Location;
                schedule.UpdatedDate = DateTime.Now;

                _context.Update(schedule);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {currentUser.Email} đã cập nhật lịch làm việc #{id}");
                TempData["SuccessMessage"] = "Đã cập nhật lịch làm việc thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật lịch làm việc");
                ModelState.AddModelError("", "Có lỗi xảy ra, vui lòng thử lại!");
                await LoadDropdownData(model, currentUser);
                return View(model);
            }
        }

        // POST: Schedule/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var schedule = await _context.WorkSchedules.FindAsync(id);
            if (schedule == null) return NotFound();

            if (!User.IsInRole("Admin") && schedule.CreatedByUserId != currentUser.Id)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xóa lịch này!";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.WorkSchedules.Remove(schedule);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {currentUser.Email} đã xóa lịch làm việc #{id}");
                TempData["SuccessMessage"] = "Đã xóa lịch làm việc thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa lịch làm việc");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa lịch!";
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper: Load dropdown data
        private async Task LoadDropdownData(CreateScheduleViewModel model, ApplicationUser currentUser)
        {
            if (User.IsInRole("Admin"))
            {
                model.AvailableUsers = await _userManager.Users.OrderBy(u => u.FullName).ToListAsync();
                model.AvailableProjects = await _context.Projects.OrderBy(p => p.Name).ToListAsync();
            }
            else
            {
                var managedProjectIds = await _context.ProjectMembers
                    .Where(pm => pm.UserId == currentUser.Id && pm.Role == "Manager")
                    .Select(pm => pm.ProjectId)
                    .ToListAsync();

                var memberIds = await _context.ProjectMembers
                    .Where(pm => managedProjectIds.Contains(pm.ProjectId))
                    .Select(pm => pm.UserId)
                    .Distinct()
                    .ToListAsync();

                model.AvailableUsers = await _userManager.Users
                    .Where(u => memberIds.Contains(u.Id))
                    .OrderBy(u => u.FullName)
                    .ToListAsync();

                model.AvailableProjects = await _context.Projects
                    .Where(p => managedProjectIds.Contains(p.Id))
                    .OrderBy(p => p.Name)
                    .ToListAsync();
            }
        }
        // GET: Schedule/Notifications - Xem thông báo của mình
        public async Task<IActionResult> Notifications()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == currentUser.Id)
                .OrderByDescending(n => n.CreatedDate)
                .Select(n => new NotificationViewModel
                {
                    Id = n.Id,
                    Title = n.Title,
                    Content = n.Content,
                    Type = n.Type,
                    RelatedUrl = n.RelatedUrl,
                    IsRead = n.IsRead,
                    CreatedDate = n.CreatedDate,
                    TimeAgo = GetTimeAgo(n.CreatedDate)
                })
                .ToListAsync();

            return View(notifications);
        }

        // GET: Schedule/GetNotificationCount - Lấy số thông báo chưa đọc (JSON)
        public async Task<IActionResult> GetNotificationCount()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Json(new { count = 0 });

            var count = await _context.Notifications
                .CountAsync(n => n.UserId == currentUser.Id && !n.IsRead);

            return Json(new { count });
        }

        // POST: Schedule/MarkNotificationAsRead
        [HttpPost]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == currentUser.Id);

            if (notification != null)
            {
                notification.IsRead = true;
                _context.Update(notification);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        // POST: Schedule/MarkAllNotificationsAsRead
        [HttpPost]
        public async Task<IActionResult> MarkAllNotificationsAsRead()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == currentUser.Id && !n.IsRead)
                .ToListAsync();

            foreach (var n in notifications)
            {
                n.IsRead = true;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã đánh dấu tất cả là đã đọc!";

            return RedirectToAction(nameof(Notifications));
        }

        // POST: Schedule/DeleteNotification
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == currentUser.Id);

            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa thông báo!";
            }

            return RedirectToAction(nameof(Notifications));
        }

        // GET: Schedule/CreateNotification - Admin gửi thông báo
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateNotification()
        {
            var model = new CreateNotificationViewModel
            {
                AvailableUsers = await _userManager.Users.OrderBy(u => u.FullName).ToListAsync()
            };
            return View(model);
        }

        // POST: Schedule/CreateNotification
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateNotification(CreateNotificationViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            if (!model.SendToAll && string.IsNullOrEmpty(model.UserId))
            {
                ModelState.AddModelError("UserId", "Vui lòng chọn người nhận hoặc chọn Gửi cho tất cả!");
            }

            if (!ModelState.IsValid)
            {
                model.AvailableUsers = await _userManager.Users.OrderBy(u => u.FullName).ToListAsync();
                return View(model);
            }

            try
            {
                if (model.SendToAll)
                {
                    // Gửi cho tất cả user
                    var allUsers = await _userManager.Users.ToListAsync();
                    foreach (var user in allUsers)
                    {
                        _context.Notifications.Add(new Notification
                        {
                            UserId = user.Id,
                            Title = model.Title,
                            Content = model.Content,
                            Type = model.Type,
                            RelatedUrl = model.RelatedUrl,
                            CreatedDate = DateTime.Now
                        });
                    }
                    _logger.LogInformation($"Admin {currentUser.Email} đã gửi thông báo cho tất cả ({allUsers.Count} người)");
                    TempData["SuccessMessage"] = $"Đã gửi thông báo cho {allUsers.Count} người!";
                }
                else
                {
                    // Gửi cho 1 người
                    _context.Notifications.Add(new Notification
                    {
                        UserId = model.UserId,
                        Title = model.Title,
                        Content = model.Content,
                        Type = model.Type,
                        RelatedUrl = model.RelatedUrl,
                        CreatedDate = DateTime.Now
                    });
                    _logger.LogInformation($"Admin {currentUser.Email} đã gửi thông báo cho user {model.UserId}");
                    TempData["SuccessMessage"] = "Đã gửi thông báo thành công!";
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Notifications));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi thông báo");
                ModelState.AddModelError("", "Có lỗi xảy ra, vui lòng thử lại!");
                model.AvailableUsers = await _userManager.Users.OrderBy(u => u.FullName).ToListAsync();
                return View(model);
            }
        }

        // Helper: Tính thời gian trước
        private static string GetTimeAgo(DateTime date)
        {
            var span = DateTime.Now - date;
            if (span.TotalMinutes < 1) return "Vừa xong";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} phút trước";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours} giờ trước";
            if (span.TotalDays < 7) return $"{(int)span.TotalDays} ngày trước";
            if (span.TotalDays < 30) return $"{(int)(span.TotalDays / 7)} tuần trước";
            return date.ToString("dd/MM/yyyy");
        }
    }
}
