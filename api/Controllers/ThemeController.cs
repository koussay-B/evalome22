using System.Threading.Tasks;
using api.data.Entities;
using api.data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ThemeController(IRepositoryWrapper repo) : ControllerBase
    {
        private readonly IRepositoryWrapper _repo = repo;

        [HttpGet("roots")]
        [Authorize(Roles = "Admin,CompanyAdmin,Formateur")]
        public async Task<IActionResult> GetRootThemes()
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null || user.CompanyId == null) return Unauthorized();

            var themes = await _repo.Theme.GetRootThemes((int)user.CompanyId);
            return Ok(themes);
        }

        [HttpGet("{id}/children")]
        [Authorize(Roles = "Admin,CompanyAdmin,Formateur")]
        public async Task<IActionResult> GetChildren(int id)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var theme = await _repo.Theme.Get(id);
            if (theme == null) return NotFound();
            if (theme.CompanyId != user.CompanyId) return Forbid();

            var children = await _repo.Theme.GetChildren(id);
            return Ok(children);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,CompanyAdmin,Formateur")]
        public async Task<IActionResult> GetTheme(int id)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var theme = await _repo.Theme.Get(id);
            if (theme == null) return NotFound();
            if (theme.CompanyId != user.CompanyId) return Forbid();

            return Ok(theme);
        }

        [HttpPost]
        [Authorize(Roles = "CompanyAdmin")]
        public async Task<IActionResult> CreateTheme([FromBody] Theme themeIn)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (themeIn.ParentId.HasValue)
            {
                var parent = await _repo.Theme.Get(themeIn.ParentId.Value);
                if (parent == null) return BadRequest("Parent theme not found");
                if (parent.CompanyId != user.CompanyId) return Forbid();
            }

            if (user.CompanyId == null) return Unauthorized();
            themeIn.CompanyId = (int)user.CompanyId;
            themeIn.CreatedById = user.Id;

            var created = await _repo.Theme.Create(themeIn);
            return Ok(created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "CompanyAdmin")]
        public async Task<IActionResult> UpdateTheme(int id, [FromBody] Theme themeIn)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var theme = await _repo.Theme.Get(id);
            if (theme == null) return NotFound();
            if (theme.CompanyId != user.CompanyId) return Forbid();

            // Only CompanyAdmin can edit any in company
            theme.Name = themeIn.Name;
            theme.Description = themeIn.Description;
            theme.Icon = themeIn.Icon;
            theme.Order = themeIn.Order;
            theme.ParentId = themeIn.ParentId;

            var updated = await _repo.Theme.Update(theme);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "CompanyAdmin")]
        public async Task<IActionResult> DeleteTheme(int id)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var theme = await _repo.Theme.Get(id);
            if (theme == null) return NotFound();
            if (theme.CompanyId != user.CompanyId) return Forbid();

            var questions = await _repo.Question.GetByTheme(id);
            if (questions.Count > 0)
                return BadRequest("Cannot delete theme with associated questions.");

            await _repo.Theme.Delete(id);
            return Ok();
        }

        // ── Admin-only endpoints (cross-company) ─────────────────────────────

        /// <summary>Returns all root themes across every company.</summary>
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminGetAllThemes()
        {
            var themes = await _repo.Theme.GetAllRootThemes();
            return Ok(themes);
        }

        /// <summary>Admin creates a theme for any company (companyId must be in body).</summary>
        [HttpPost("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminCreateTheme([FromBody] Theme themeIn)
        {
            if (themeIn.CompanyId == 0)
                return BadRequest("CompanyId is required.");

            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (themeIn.ParentId.HasValue)
            {
                var parent = await _repo.Theme.Get(themeIn.ParentId.Value);
                if (parent == null) return BadRequest("Parent theme not found.");
            }

            themeIn.CreatedById = user.Id;
            var created = await _repo.Theme.Create(themeIn);
            return Ok(created);
        }

        /// <summary>Admin updates any theme regardless of company.</summary>
        [HttpPut("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminUpdateTheme(int id, [FromBody] Theme themeIn)
        {
            var theme = await _repo.Theme.Get(id);
            if (theme == null) return NotFound();

            theme.Name        = themeIn.Name;
            theme.Description = themeIn.Description;
            theme.Icon        = themeIn.Icon;
            theme.Order       = themeIn.Order;
            theme.ParentId    = themeIn.ParentId;

            var updated = await _repo.Theme.Update(theme);
            return Ok(updated);
        }

        /// <summary>Admin deletes any theme regardless of company.</summary>
        [HttpDelete("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDeleteTheme(int id)
        {
            var theme = await _repo.Theme.Get(id);
            if (theme == null) return NotFound();

            var questions = await _repo.Question.GetByTheme(id);
            if (questions.Count > 0)
                return BadRequest("Cannot delete theme with associated questions.");

            await _repo.Theme.Delete(id);
            return Ok();
        }
    }
}
