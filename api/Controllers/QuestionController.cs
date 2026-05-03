using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.data.Entities;
using api.data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionController(IRepositoryWrapper repo) : ControllerBase
    {
        private readonly IRepositoryWrapper _repo = repo;

        [HttpGet]
        [Authorize(Roles = "Admin,CompanyAdmin,Formateur")]
        public async Task<IActionResult> GetQuestions()
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null || user.CompanyId == null) return Unauthorized();

            var questions = await _repo.Question.GetByCompany((int)user.CompanyId);
            return Ok(questions);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,CompanyAdmin,Formateur")]
        public async Task<IActionResult> GetQuestion(int id)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var question = await _repo.Question.Get(id); // Custom Get not fully complete in repo, but Get() works, ideally we used Include
            // To properly load Choices & Prerequisites, we can just use the context or assume Get(id) needs an override.
            // Using a query for fully included entity
            question = _repo.Question.GetAllAsQueryable()
                .Where(q => q.Id == id && q.Enable)
                .Select(q => new Question
                {
                    Id = q.Id,
                    Title = q.Title,
                    Explanation = q.Explanation,
                    Hint = q.Hint,
                    Type = q.Type,
                    Complexity = q.Complexity,
                    TimeLimitSeconds = q.TimeLimitSeconds,
                    Points = q.Points,
                    CompanyId = q.CompanyId,
                    CreatedById = q.CreatedById,
                    ThemeId = q.ThemeId,
                    Choices = q.Choices.Where(c => c.Enable).ToList(),
                    Prerequisites = q.Prerequisites.Where(p => p.Enable).ToList()
                })
                .FirstOrDefault();

            if (question == null) return NotFound();
            if (question.CompanyId != user.CompanyId) return Forbid();

            return Ok(question);
        }

        [HttpGet("theme/{themeId}")]
        [Authorize(Roles = "Admin,CompanyAdmin,Formateur")]
        public async Task<IActionResult> GetByTheme(int themeId)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var theme = await _repo.Theme.Get(themeId);
            if (theme == null) return NotFound();
            if (theme.CompanyId != user.CompanyId) return Forbid();

            var questions = await _repo.Question.GetByTheme(themeId);
            return Ok(questions);
        }

        [HttpPost]
        [Authorize(Roles = "CompanyAdmin,Formateur")]
        public async Task<IActionResult> CreateQuestion([FromBody] Question questionIn)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Validate theme
            var theme = await _repo.Theme.Get(questionIn.ThemeId);
            if (theme == null) return BadRequest("Theme not found");
            if (theme.CompanyId != user.CompanyId) return Forbid();

            if (user.CompanyId == null) return Unauthorized();
            questionIn.CompanyId = (int)user.CompanyId;
            questionIn.CreatedById = user.Id;

            if (questionIn.Type == QuestionType.TrueFalse)
            {
                questionIn.Choices = new List<QuestionChoice>
                {
                    new QuestionChoice { Text = "True", IsCorrect = questionIn.Choices.FirstOrDefault(c => c.Text.Equals("True", System.StringComparison.OrdinalIgnoreCase))?.IsCorrect ?? false, Order = 1 },
                    new QuestionChoice { Text = "False", IsCorrect = questionIn.Choices.FirstOrDefault(c => c.Text.Equals("False", System.StringComparison.OrdinalIgnoreCase))?.IsCorrect ?? false, Order = 2 }
                };
            }

            var created = await _repo.Question.Create(questionIn);
            return Ok(created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "CompanyAdmin,Formateur")]
        public async Task<IActionResult> UpdateQuestion(int id, [FromBody] Question questionIn)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var question = await _repo.Question.Get(id);
            if (question == null) return NotFound();
            if (question.CompanyId != user.CompanyId) return Forbid();

            var isCompanyAdmin = await _repo.UserManager.IsInRoleAsync(user, "CompanyAdmin");
            if (!isCompanyAdmin && question.CreatedById != user.Id) return Forbid();

            question.Title = questionIn.Title;
            question.Explanation = questionIn.Explanation;
            question.Hint = questionIn.Hint;
            question.Type = questionIn.Type;
            question.Complexity = questionIn.Complexity;
            question.TimeLimitSeconds = questionIn.TimeLimitSeconds;
            question.Points = questionIn.Points;
            question.ThemeId = questionIn.ThemeId;

            // Simple choice update is missing diff logic here, typically handled separately or by replacing full list.

            var updated = await _repo.Question.Update(question);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "CompanyAdmin,Formateur")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var question = await _repo.Question.Get(id);
            if (question == null) return NotFound();
            if (question.CompanyId != user.CompanyId) return Forbid();

            var isCompanyAdmin = await _repo.UserManager.IsInRoleAsync(user, "CompanyAdmin");
            if (!isCompanyAdmin && question.CreatedById != user.Id) return Forbid();

            await _repo.Question.Delete(id);
            return Ok();
        }
    }
}
