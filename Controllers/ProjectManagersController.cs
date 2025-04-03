using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Models.EFModels;
using ProjectManagementSystem.Models.ViewModels;

namespace ProjectManagementSystem.Controllers
{
    public class ProjectManagersController : Controller
    {
        private readonly AppDbContext _context;

        public ProjectManagersController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var managers = _context.ProjectManagers
            .Select(m => new ProjectManagerVm
             {
                 ManagerId = m.ManagerId,
                 Name = m.Name,
                 ActiveProjectsCount = m.Projects.Count(p=>p.Status=="進行中")
             })
            .ToList();

            return View(managers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectManagerVm vm)
        {
            if (ModelState.IsValid)
            {
                var manager = new ProjectManager
                {
                    Name = vm.Name
                };
                _context.ProjectManagers.Add(manager);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProjectManagerVm vm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var manager = await _context.ProjectManagers.FindAsync(vm.ManagerId);
                    if (manager == null) return NotFound();

                    manager.Name = vm.Name;
                    _context.Update(manager);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "更新失敗: " + ex.Message);
                }
            }
            return PartialView("_EditManagerModal", vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var manager = await _context.ProjectManagers.FindAsync(id);
                if (manager == null) return NotFound();

                var hasProjects = await _context.Projects
                    .AnyAsync(P=>P.OwnerId == id);

                if (hasProjects)
                {
                    TempData["Error"]="無法刪除專案經理，因為有專案屬於此專案經理";
                    return RedirectToAction(nameof(Index));
                }

                _context.ProjectManagers.Remove(manager);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }
}
