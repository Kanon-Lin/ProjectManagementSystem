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

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            var totalProjects = await _context.Projects.CountAsync();

            var projects = await _context.Projects
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(p => p.Owner)   // 使用 Include 來加入 ProjectManagers 表，取得專案經理的名字。Owner 是指向 ProjectManager 的導航屬性
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalProjects / pageSize);

            var model = new ProjectListVm
            {
                Projects = projects ?? new List<Project>(),  // 確保 Projects 初始化為空列表，而不是 null
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return View(model);  // 返回 View，並傳遞專案列表
        }

        // GET: Projects/Create
        public IActionResult Create()
        {
            var model = new ProjectCreateVm
            {
                StartDate = DateTime.Today
            };

            // 使用 ViewBag 來處理下拉選單
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

