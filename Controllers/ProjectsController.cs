using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Models.EFModels;
using ProjectManagementSystem.Models.ViewModels;
using System.Diagnostics;
using System.Threading.Tasks;
using ProjectManagementSystem.Models.Dtos;

namespace ProjectManagementSystem.Controllers
{
    public class ProjectsController : Controller

    {
        private readonly AppDbContext _context;

        public ProjectsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchTerm,
            DateTime? startDateFilter,
            DateTime? endDateFilter,
            string statusFilter,
            int? managerFilter,
            string sortOrder,
            int pageNumber = 1,
            int pageSize = 10)
        {
            // 設置排序參數
            ViewBag.NameSortParam = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.StartDateSortParam = sortOrder == "startDate" ? "startDate_desc" : "startDate";
            ViewBag.EndDateSortParam = sortOrder == "endDate" ? "endDate_desc" : "endDate";
            ViewBag.StatusSortParam = sortOrder == "status" ? "status_desc" : "status";
            ViewBag.ManagerSortParam = sortOrder == "manager" ? "manager_desc" : "manager";

            // 基礎查詢
            var query = _context.Projects
                .Include(p => p.Owner)
                .AsQueryable();

            // 套用搜尋條件
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm));
            }

            // 套用篩選條件
            if (startDateFilter.HasValue)
            {
                query = query.Where(p => p.StartDate >= startDateFilter.Value);
            }

            if (endDateFilter.HasValue)
            {
                query = query.Where(p => p.EndDate <= endDateFilter.Value);
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                query = query.Where(p => p.Status == statusFilter);
            }

            if (managerFilter.HasValue)
            {
                query = query.Where(p => p.OwnerId == managerFilter.Value);
            }

            // 套用排序
            query = sortOrder switch
            {
                "name_desc" => query.OrderByDescending(p => p.Name),
                "startDate" => query.OrderBy(p => p.StartDate),
                "startDate_desc" => query.OrderByDescending(p => p.StartDate),
                "endDate" => query.OrderBy(p => p.EndDate),
                "endDate_desc" => query.OrderByDescending(p => p.EndDate),
                "status" => query.OrderBy(p => p.Status),
                "status_desc" => query.OrderByDescending(p => p.Status),
                "manager" => query.OrderBy(p => p.Owner.Name),
                "manager_desc" => query.OrderByDescending(p => p.Owner.Name),
                _ => query.OrderBy(p => p.Name)
            };

            // 計算總數並套用分頁
            var totalProjects = await query.CountAsync();
            var projects = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 準備下拉選單選項
            var statusOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "All Status" }
            }
            .Union(new[]
            {
                new SelectListItem { Value = "未開始", Text = "未開始" },
                new SelectListItem { Value = "進行中", Text = "進行中" },
                new SelectListItem { Value = "已完成", Text = "已完成" },
                new SelectListItem { Value = "已終止", Text = "已終止" },
                new SelectListItem { Value = "已取消", Text = "已取消" }
            })
            .ToList();

            var projectManagers = await _context.ProjectManagers
                .Select(pm => new SelectListItem
                {
                    Value = pm.ManagerId.ToString(),
                    Text = pm.Name
                })
                .ToListAsync();

            projectManagers = new List<SelectListItem>
            {
                new SelectListItem{Value="",Text="All Managers"}
            }
            .Union(projectManagers).ToList();

            var model = new ProjectListVm
            {
                Projects = projects,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalProjects / pageSize),
                SearchTerm = searchTerm,
                StartDateFilter = startDateFilter,
                EndDateFilter = endDateFilter,
                StatusFilter = statusFilter,
                ManagerFilter = managerFilter,
                SortOrder = sortOrder,
                StatusOptions = statusOptions,
                ProjectManagers = projectManagers
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new ProjectCreateVm
            {
                StartDate = DateTime.Today
            };

            ViewBag.ProjectManagers = new SelectList(
                _context.ProjectManagers,
                "ManagerId",
                "Name"
            );

            ViewBag.StatusOptions = new List<string> { "未開始", "進行中", "已完成", "已終止", "已取消" };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectCreateVm vm)
        {
            Debug.WriteLine("=== Form Submission Debug Info ===");
            Debug.WriteLine($"OwnerId: {vm.OwnerId}");
            Debug.WriteLine($"Name: {vm.Name}");
            Debug.WriteLine($"Status: {vm.Status}");
            Debug.WriteLine($"StartDate: {vm.StartDate}");

            if (ModelState.IsValid)
            {
                try
                {
                    var project = new Project
                    {
                        Name = vm.Name.Trim(),
                        Description = vm.Description?.Trim(),
                        Status = vm.Status,
                        StartDate = vm.StartDate,
                        EndDate = vm.EndDate,
                        OwnerId = vm.OwnerId
                    };

                    _context.Projects.Add(project);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Project created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception: {ex.Message}");
                    ModelState.AddModelError("", $"An error occurred while creating the project: {ex.Message}");
                }
            }

            // 如果到達這裡，表示需要重新顯示表單
            ViewBag.ProjectManagers = new SelectList(
                _context.ProjectManagers,
                "ManagerId",
                "Name",
                vm.OwnerId
            );

            ViewBag.StatusOptions = new List<string>
        {
            "未開始",
            "進行中",
            "已完成",
            "已終止",
            "已取消"
        };

            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Owner)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.AssignedTo)
                .FirstOrDefaultAsync(p => p.ProjectId == id);

            if (project == null) return NotFound();

            var viewModel = new ProjectDetailsVm
            {
                ProjectId = project.ProjectId,
                Name = project.Name,
                Description = project.Description,
                Status = project.Status,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                OwnerId = project.OwnerId,
                OwnerName = project.Owner?.Name,
                Tasks = project.Tasks?.Select(t => new TaskDto
                {
                    TaskId = t.TaskId,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    DueDate = t.DueDate,
                    Priority = t.Priority,
                    AssignedToId = t.AssignedToId ?? 0,
                    AssignedToName = t.AssignedTo?.Name
                }).ToList() ?? new List<TaskDto>(),

                ProjectManagers = await _context.ProjectManagers
                    .Select(pm => new SelectListItem
                    {
                        Value = pm.ManagerId.ToString(),
                        Text = pm.Name,
                        Selected = pm.ManagerId == project.OwnerId
                    })
                    .ToListAsync(),

                StatusOptions = new List<SelectListItem>
        {
           new SelectListItem { Value = "未開始", Text = "未開始" },
        new SelectListItem { Value = "進行中", Text = "進行中" },
        new SelectListItem { Value = "已完成", Text = "已完成" },
        new SelectListItem { Value = "已取消", Text = "已取消" }
        },

                // 添加 TaskCreateVm 初始化
                TaskCreateVm = new TaskCreateVm
                {
                    ProjectId = id,
                    TeamMembers = await _context.TeamMembers
                        .Select(m => new SelectListItem
                        {
                            Value = m.MemberId.ToString(),
                            Text = m.Name
                        })
                        .ToListAsync(),
                    Statuses = new List<SelectListItem>
            {
                new SelectListItem { Value = "未開始", Text = "未開始" },
                new SelectListItem { Value = "進行中", Text = "進行中" },
                new SelectListItem { Value = "已完成", Text = "已完成" }
            },
                    Priorities = new List<SelectListItem>
            {
               new SelectListItem { Value = "高", Text = "高" },
            new SelectListItem { Value = "中", Text = "中" },
            new SelectListItem { Value = "低", Text = "低" }
            }
                }
            };

            return View(viewModel);
        }
    }
}

