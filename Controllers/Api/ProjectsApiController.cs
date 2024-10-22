using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Models.EFModels;

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

		//添加分頁機制來優化性能，避免一次性查詢太多數據。
		// GET: api/ProjectsApi
		[HttpGet]
		public async Task<ActionResult<IEnumerable<Project>>> GetProjects([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
		{
			var projects = await _context.Projects
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return Ok(projects);
		}

		// GET: api/ProjectsApi/5
		[HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project == null)
            {
                return NotFound(new { message = "Project not found." });// 提供更具描述性的錯誤訊息
			}

			return Ok(project);
		}

        // PUT: api/ProjectsApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProject(int id, Project project)
        {
			if (id != project.ProjectId)
			{
				return BadRequest("The project ID in the URL and body must match.");
			}

			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);  // 模型驗證失敗時返回錯誤
			}


			_context.Entry(project).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
			catch (DbUpdateConcurrencyException ex)
			{
				if (!ProjectExists(id))
				{
					return NotFound();
				}
				else
				{
					return StatusCode(500, new { message = "A concurrency error occurred.", detail = ex.Message });
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "An error occurred while updating the project.", detail = ex.Message });
			}

			return NoContent();
        }

		//當需要更新部分屬性時，PUT 要求必須傳遞整個對象，這在某些場景下可能不那麼靈活。
		//你可以考慮為部分更新添加 PATCH 支援。這會允許你只更新某些屬性，而不需要提供整個對象。
		[HttpPatch("{id}")]
		public async Task<IActionResult> PatchProject(int id, [FromBody] JsonPatchDocument<Project> patchDoc)
		{
			if (patchDoc == null)
			{
				return BadRequest();
			}

			var project = await _context.Projects.FindAsync(id);
			if (project == null)
			{
				return NotFound();
			}
			try
			{
				patchDoc.ApplyTo(project);  // 不再使用 ModelState，使用內部驗證
				await _context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}

			await _context.SaveChangesAsync();

			return NoContent();
		}



		// POST: api/ProjectsApi
		// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
		[HttpPost]
        public async Task<ActionResult<Project>> PostProject(Project project)
        {
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);  // 模型驗證失敗時返回錯誤
			}
			_context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProject", new { id = project.ProjectId }, project);
        }

        // DELETE: api/ProjectsApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
				return NotFound(new { message = "Project not found." }); // 提供更具描述性的錯誤訊息
			}

            _context.Projects.Remove(project);
			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException ex)
			{
				if (!ProjectExists(id))
				{
					return NotFound();
				}
				else
				{
					return StatusCode(500, new { message = "A concurrency error occurred.", detail = ex.Message });
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "An error occurred while updating the project.", detail = ex.Message });
			}


			return NoContent();
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.ProjectId == id);
        }
    }
}
