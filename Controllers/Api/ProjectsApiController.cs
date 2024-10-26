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

        public ProjectsApiController(AppDbContext context)
        {
            _context = context;
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
                Name = project.Name,
                Description = project.Description,
                Status = project.Status,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                OwnerName = project.Owner?.Name,
                Tasks = project.Tasks.Select(t => new TaskDto
                {
                    TaskId = t.TaskId,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    DueDate = t.DueDate,
                    Priority = t.Priority,
                    AssignedToName = t.AssignedTo?.Name
                }).ToList()
            };
        }

        // PUT: api/projects/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, ProjectUpdateDto projectDto)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            project.Name = projectDto.Name;
            project.Description = projectDto.Description;
            project.Status = projectDto.Status;
            project.OwnerId = projectDto.OwnerId;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
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
        public async Task<ActionResult<TaskDto>> CreateTask(int projectId, TaskCreateDto taskDto)
        {
            var task = new ProjectTask
            {
                ProjectId = projectId,
                Title = taskDto.Title,
                Description = taskDto.Description,
                Status = taskDto.Status,
                DueDate = taskDto.DueDate,
                Priority = taskDto.Priority,
                AssignedToId = taskDto.AssignedToId
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProject), new { id = projectId }, task);
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.ProjectId == id);
        }
    }
}