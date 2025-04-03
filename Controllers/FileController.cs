using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Models.EFModels;
using ProjectManagementSystem.Models.ViewModels;
using System.Threading.Tasks;
using File = ProjectManagementSystem.Models.EFModels.File;
using Task = System.Threading.Tasks.Task;

namespace ProjectManagementSystem.Controllers
{
    public class FileController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public FileController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // 1. 檔案列表與上傳頁面
        public async Task<ActionResult> Index(int taskId)
        {
            var files = await GetFileListAsync(taskId);
            var vm = new FileVm
            {
                TaskId = taskId,
                Files = files
            };
            return View(vm);
        }

        // 2. 檔案上傳
        [HttpPost]
        public async Task<ActionResult> Upload(FileVm model)
        {
            if (model.UploadFile != null)
            {
                var filePath = await SaveFileAsync(model.UploadFile);
                await SaveFileInfoAsync(model, filePath);
            }
            return RedirectToAction(nameof(Index), new { taskId = model.TaskId });
        }

        // 3. 檔案下載
        public async Task<ActionResult> Download(int id)
        {
            var fileInfo = await GetFileInfoAsync(id);
            if (fileInfo == null) return NotFound();

            var fileBytes = await System.IO.File.ReadAllBytesAsync(fileInfo.FilePath);
            return File(fileBytes, "application/octet-stream", fileInfo.FileName);
        }

        // 4. 檔案刪除
        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            var taskId = 0;

            try
            {
                var file = await _context.Files.FindAsync();
                if (file == null) return NotFound();

                taskId = file.TaskId;

                if (System.IO.File.Exists(file.FilePath))
                {
                    System.IO.File.Delete(file.FilePath);
                }

                _context.Files.Remove(file);
                await _context.SaveChangesAsync();

                TempData["Message"] = "檔案已成功刪除";
                return RedirectToAction(nameof(Index), new { taskId = file.TaskId });

            }
            catch (Exception ex)
            {
                TempData["Message"] = "刪除檔案時發生錯誤";
            }
            return RedirectToAction(nameof(Index), new { taskId }); 

        }

        #region Private Methods
        // 取得檔案列表
        private async Task<List<FileVm>> GetFileListAsync(int taskId)
        {
            return await _context.Files
                .Where(f => f.TaskId == taskId)
                .Include(f => f.UploadedBy)
                .Select(f => new FileVm
                {
                    FileId = f.FileId,
                    FileName = f.FileName,
                    UploadedByName = f.UploadedBy.Name,
                    UploadedAt = f.UploadedAt
                })
                .ToListAsync();
        }

        // 儲存實體檔案
        private async Task<string> SaveFileAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            Directory.CreateDirectory(uploadsFolder); // 確保資料夾存在
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return filePath;
        }

        // 儲存檔案資訊到資料庫
        private async Task SaveFileInfoAsync(FileVm model, string filePath)
        {
            var file = new File
            {
                TaskId = model.TaskId,
                FileName = model.UploadFile.FileName,
                FilePath = filePath,
                UploadedById = 1, // 這裡要改成實際的使用者ID
                UploadedAt = DateTime.Now
            };

            _context.Files.Add(file);
            await _context.SaveChangesAsync();
            return;
        }

        // 取得檔案資訊
        private async Task<File> GetFileInfoAsync(int id)
        {
            return await _context.Files.FindAsync(id);
        }
        #endregion
    }
    }