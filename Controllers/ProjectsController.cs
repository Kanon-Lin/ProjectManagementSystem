using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Models.EFModels;
using ProjectManagementSystem.Models.ViewModels;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ProjectManagementSystem.Controllers
{
	public class ProjectsController : Controller

	{
		private readonly AppDbContext _context;

		public ProjectsController(AppDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index(int pageNumber = 1, int pageSize =10)
		{
            var totalProjects = await _context.Projects.CountAsync();

            var projects = await _context.Projects
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
                .Include(p=> p.Owner)   // 使用 Include 來加入 ProjectManagers 表，取得專案經理的名字。Owner 是指向 ProjectManager 的導航屬性
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
            // 提供專案狀態選項
            ViewBag.StatusOptions = new List<string> { "Not Started", "In Progress", "Completed", "Terminated", "Cancelled" };

            // 從 ProjectManagers 表中取得所有專案經理
            var projectManagers = _context.ProjectManagers.ToList();
            if (projectManagers != null && projectManagers.Any())
            {
                // 直接建立 SelectList 並指定正確的值和顯示欄位
                ViewBag.ProjectManagers = new SelectList(projectManagers, "ManagerId", "Name");
                // 或者使用 ViewData
                // ViewData["ProjectManagers"] = new SelectList(projectManagers, "ManagerId", "Name");
            }
            else
            {
                ViewBag.ProjectManagers = new SelectList(Enumerable.Empty<SelectListItem>());
            }

            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectCreateVm vm)
        {
            try
            {
                // 添加偵錯信息
    Debug.WriteLine($"Received POST request for project creation:");
                Debug.WriteLine($"Name: {vm.Name}");
                Debug.WriteLine($"OwnerId: {vm.OwnerId}");
                Debug.WriteLine($"Status: {vm.Status}");
                Debug.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

                if (ModelState.IsValid)
                {
                    var project = new Project
                    {
                        Name = vm.Name,
                        Description = vm.Description,
                        Status = vm.Status,
                        StartDate = vm.StartDate,
                        EndDate = vm.EndDate,
                        OwnerId = vm.OwnerId,
                    };

                    _context.Projects.Add(project);
                    await _context.SaveChangesAsync();

                    // 可以添加成功消息
                    TempData["Success"] = "Project created successfully!";

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                     foreach (var modelStateKey in ModelState.Keys)
        {
            var modelStateVal = ModelState[modelStateKey];
            foreach (var error in modelStateVal.Errors)
            {
                Debug.WriteLine($"Key: {modelStateKey}, Error: {error.ErrorMessage}");
            }
        }
                }

            }
            catch (Exception ex)
            {
                // 記錄錯誤
                ModelState.AddModelError("", "An error occurred while creating the project.");
            }

            // 如果出錯或驗證失敗，重新加載必要的數據
            ViewBag.StatusOptions = new List<string> { "Not Started", "In Progress", "Completed", "Terminated", "Cancelled" };

            // 重新加載 ProjectManagers 下拉選單數據
            var projectManagers = await _context.ProjectManagers.ToListAsync();
            if (projectManagers != null && projectManagers.Any())
            {
                ViewBag.ProjectManagers = new SelectList(projectManagers, "ManagerId", "Name", vm.OwnerId);
            }
            else
            {
                ViewBag.ProjectManagers = new SelectList(Enumerable.Empty<SelectListItem>());
            }

            // 返回表單視圖
            return View(vm);
        }

    }
}
