using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Models.Dtos;
using ProjectManagementSystem.Models.EFModels;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ProjectTask = ProjectManagementSystem.Models.EFModels.Task;


namespace ProjectManagementSystem.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IValidator<TaskCreateDto> _validator;
        private readonly ILogger<ProjectsApiController> _logger;
        private readonly IValidator<TaskUpdateDto> _updateValidator;

        public ProjectsApiController(AppDbContext context,
        IValidator<TaskCreateDto> validator,
        ILogger<ProjectsApiController> logger,
        IValidator<TaskUpdateDto> updateValidator)
        {
            _context = context;
            _validator = validator;
            _logger = logger;
            _updateValidator = updateValidator;
        }

        // GET: api/projects/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDetailsDto>> GetProject(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Owner)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.AssignedTo)
                .FirstOrDefaultAsync(p => p.ProjectId == id);

            if (project == null)
            {
                return NotFound();
            }

            return new ProjectDetailsDto
            {
                ProjectId = project.ProjectId,
                Name = project.Name ?? string.Empty,
                Description = project.Description ?? string.Empty,
                Status = project.Status ?? string.Empty,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                OwnerId = project.OwnerId,
                OwnerName = project.Owner?.Name ?? string.Empty,
                Tasks = project.Tasks.Select(t => new TaskDto
                {
                    TaskId = t.TaskId,
                    Title = t.Title ?? string.Empty,
                    Description = t.Description ?? string.Empty,
                    Status = t.Status ?? string.Empty,
                    DueDate = t.DueDate,
                    Priority = t.Priority ?? string.Empty,
                    AssignedToId = t.AssignedToId ?? 0,
                    AssignedToName = t.AssignedTo?.Name ?? string.Empty
                }).ToList() ?? new List<TaskDto>()
            };
        }

        // PUT: api/projects/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, ProjectUpdateDto projectDto)
        {
            var project = await _context.Projects
                .Include(p => p.Owner) // 確保包含 Owner 關聯
                .FirstOrDefaultAsync(p => p.ProjectId == id);

            if (project == null)
            {
                return NotFound("找不到指定的專案");
            }

            project.Name = projectDto.Name;
            project.Description = projectDto.Description;
            project.Status = projectDto.Status;
            project.OwnerId = projectDto.OwnerId;
            project.StartDate = projectDto.StartDate;
            project.EndDate = projectDto.EndDate;

            try
            {
                await _context.SaveChangesAsync();

                // 更新後重新查詢以獲取最新的 Owner 資料
                project = await _context.Projects
                    .Include(p => p.Owner)
                    .Include(p => p.Tasks)
                        .ThenInclude(t => t.AssignedTo)
                    .FirstOrDefaultAsync(p => p.ProjectId == id);

                var updatedProject = new ProjectDetailsDto
                {
                    ProjectId = project.ProjectId,
                    Name = project.Name,
                    Description = project.Description,
                    Status = project.Status,
                    OwnerId = project.OwnerId,
                    OwnerName = project.Owner.Name, // 使用 null 條件運算子
                    StartDate = project.StartDate,
                    EndDate = project.EndDate
                };

                return Ok(updatedProject);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(id))
                {
                    return NotFound();
                }
                throw;
            }
        }

        // POST: api/projects/{id}/tasks
        [HttpPost("{projectId}/tasks")]
        public async Task<ActionResult<TaskDto>> CreateTask(int projectId, TaskCreateDto dto)
        {
            try
            {
                _logger.LogInformation($"正在為專案 {projectId} 建立新任務");

                // 1. 檢查專案是否存在
                if (!ProjectExists(projectId))
                    return NotFound(new ErrorResponseDto
                    {
                        Error = "NotFound",
                        Message = $"找不到ID為 {projectId} 的專案",
                        StatusCode = 404
                    });

                // 2. 驗證 DTO
                var validationResult = await _validator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning($"任務驗證失敗: {string.Join(", ", validationResult.Errors)}");
                    return BadRequest(new ErrorResponseDto
                    {
                        Error = "ValidationError",
                        Message = string.Join(", ", validationResult.Errors),
                        StatusCode = 400
                    });
                }

                // 3. 檢查專案狀態
                if (await IsProjectCompleted(projectId))
                    return BadRequest(new ErrorResponseDto
                    {
                        Error = "InvalidOperation",
                        Message = "已完成的專案不能新增任務",
                        StatusCode = 400
                    });

                // 4. 使用交易新增任務
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var task = new ProjectTask
                    {
                        ProjectId = projectId,
                        Title = dto.Title,
                        Description = dto.Description,
                        Status = dto.Status,
                        DueDate = dto.DueDate,
                        Priority = dto.Priority,
                        AssignedToId = dto.AssignedToId
                    };

                    _context.Tasks.Add(task);
                    await _context.SaveChangesAsync();

                    var createdTask = await _context.Tasks
                        .Include(t => t.AssignedTo)
                        .FirstOrDefaultAsync(t => t.TaskId == task.TaskId);

                    var taskDto = new TaskDto
                    {
                        TaskId = createdTask.TaskId,
                        Title = createdTask.Title,
                        Description = createdTask.Description,
                        Status = createdTask.Status,
                        DueDate = createdTask.DueDate,
                        Priority = createdTask.Priority,
                        AssignedToId = createdTask.AssignedToId ?? 0,
                        AssignedToName = createdTask.AssignedTo?.Name
                    };

                    await transaction.CommitAsync();
                    _logger.LogInformation($"已成功建立任務 ID: {taskDto.TaskId}");

                    return CreatedAtAction(
                        nameof(GetProject),
                        new { id = projectId },
                        taskDto);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立任務時發生錯誤");
                return StatusCode(500, new ErrorResponseDto
                {
                    Error = "InternalServerError",
                    Message = "建立任務時發生錯誤",
                    StatusCode = 500
                });
            }
        }

        // DELETE: api/ProjectsApi/{projectId}/tasks/{taskId}
        [HttpDelete("{projectId}/tasks/{taskId}")]
        public async Task<ActionResult<TaskDeleteDto>> DeleteTask(int projectId, int taskId)
        {
            try
            {
                //應該不需要以下，因為已在project詳情頁面
                if (!ProjectExists(projectId))
                    return NotFound(new ErrorResponseDto
                    {
                        Error = "NotFound",
                        Message = $"找不到ID為 {projectId} 的專案",
                        StatusCode = 404
                    });

                //檢查任務

                if (!await TaskExists(projectId, taskId))
                    return NotFound(new ErrorResponseDto
                    {
                        Error = "NotFound",
                        Message = $"找不到ID為 {taskId} 的任務",
                        StatusCode = 404
                    });

                //檢查status，如果是已完成就不能刪除
                if(await IsProjectCompleted(projectId))
                    return BadRequest(new ErrorResponseDto
                    {
                        Error = "InvalidOperation",
                        Message = "已完成的專案不能刪除任務",
                        StatusCode = 400
                    });

                //使用交易，獲取任務並刪除
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var task = await _context.Tasks
                        .Include(t => t.Files)
                        .Include(t => t.Notifications)
                        .Include(t => t.TaskAssignments)
                        .FirstOrDefaultAsync(t => t.ProjectId == projectId && t.TaskId == taskId);

                    var response = new TaskDeleteDto
                    {
                        TaskId = task.TaskId,
                        Title = task.Title,
                        Success = true,
                        DeletedAt = DateTime.Now
                    };

                    //刪除關聯資料
                    if (task.Files.Any())
                        _context.Files.RemoveRange(task.Files);

                    if (task.Notifications.Any())
                        _context.Notifications.RemoveRange(task.Notifications);

                    if (task.TaskAssignments.Any())
                        _context.TaskAssignments.RemoveRange(task.TaskAssignments);

                    //刪除任務
                    _context.Tasks.Remove(task);
                    await _context.SaveChangesAsync();

                    //提交交易
                    await transaction.CommitAsync();

                    _logger.LogInformation($"任務{taskId}及其關聯資料已被成功刪除");
                    return Ok(response);

                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"刪除任務 {taskId} 時發生錯誤");
                return StatusCode(500, new ErrorResponseDto
                {
                    Error = "InternalError",
                    Message = "刪除任務時發生錯誤",
                    StatusCode = 500
                });
            }
        }

        [HttpPut("{projectId}/tasks/{taskId}")]
        public async Task<ActionResult<TaskDto>> UpdateTask(int projectId, int taskId, TaskUpdateDto dto)
        {
            try
            {
                // 1. 檢查專案是否存在
                if (!ProjectExists(projectId))
                    return NotFound(new ErrorResponseDto
                    {
                        Error = "NotFound",
                        Message = $"找不到ID為 {projectId} 的專案",
                        StatusCode = 404
                    });

                // 2. 檢查任務是否存在
                if (!await TaskExists(projectId, taskId))
                    return NotFound(new ErrorResponseDto
                    {
                        Error = "NotFound",
                        Message = $"找不到ID為 {taskId} 的任務",
                        StatusCode = 404
                    });

                // 3. 檢查專案狀態
                if (await IsProjectCompleted(projectId))
                    return BadRequest(new ErrorResponseDto
                    {
                        Error = "InvalidOperation",
                        Message = "已完成的專案不能修改任務",
                        StatusCode = 400
                    });

                // 4. 驗證 DTO
                var validationResult = await _updateValidator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning($"任務驗證失敗: {string.Join(", ", validationResult.Errors)}");
                    return BadRequest(new ErrorResponseDto
                    {
                        Error = "ValidationError",
                        Message = string.Join(", ", validationResult.Errors),
                        StatusCode = 400
                    });
                }

                // 5. 使用交易更新任務
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var task = await _context.Tasks
                        .Include(t => t.AssignedTo)
                        .FirstOrDefaultAsync(t => t.ProjectId == projectId && t.TaskId == taskId);

                    // 更新任務資料
                    task.Title = dto.Title;
                    task.Description = dto.Description;
                    task.Status = dto.Status;
                    task.Priority = dto.Priority;
                    task.DueDate = dto.DueDate;
                    task.AssignedToId = dto.AssignedToId;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // 6. 回傳更新後的資料
                    return Ok(new TaskDto
                    {
                        TaskId = task.TaskId,
                        Title = task.Title,
                        Description = task.Description,
                        Status = task.Status,
                        DueDate = task.DueDate,
                        Priority = task.Priority,
                        AssignedToId = task.AssignedToId ?? 0,
                        AssignedToName = task.AssignedTo?.Name
                    });
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新任務 {taskId} 時發生錯誤");
                return StatusCode(500, new ErrorResponseDto
                {
                    Error = "InternalServerError",
                    Message = "更新任務時發生錯誤",
                    StatusCode = 500
                });
            }
        }

        private async Task<bool> TaskExists(int projectId, int taskId)
        {
            return await _context.Tasks.AnyAsync(t =>
                t.ProjectId == projectId && t.TaskId == taskId);
        }

        private async Task<bool> IsProjectCompleted(int projectId)
        {
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);
            return project?.Status == "已完成";
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.ProjectId == id);
        }
    }
}