using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Models.Dtos;
using ProjectManagementSystem.Models.EFModels;
using ProjectTask = ProjectManagementSystem.Models.EFModels.Task;


namespace ProjectManagementSystem.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IValidator<TaskCreateDto> _validator;

        public ProjectsApiController(AppDbContext context,
        IValidator<TaskCreateDto> validator)
        {
            _context = context;
            _validator = validator;

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
                    AssignedToId = t.AssignedToId,
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
                var validationResult = await _validator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { Errors = validationResult.Errors });
                }

                var project = await _context.Projects
                    .FirstOrDefaultAsync(p => p.ProjectId == projectId);

                if (project == null)
                    return NotFound("找不到指定的專案");

                if (project.Status == "Completed")
                    return BadRequest("已完成的專案不能新增任務");

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
                    AssignedToId = createdTask.AssignedToId,
                    AssignedToName = createdTask.AssignedTo.Name
                };

                return CreatedAtAction(
                    nameof(GetProject),
                    new { id = projectId },
                    taskDto);
            }
            catch (Exception)
            {
                return StatusCode(500, "建立任務時發生錯誤");
            }
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.ProjectId == id);
        }
    }
}