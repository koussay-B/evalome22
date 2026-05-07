using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.data.Entities;
using api.data.Repositories;
using api.dtos.request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionnaireController(IRepositoryWrapper repo) : ControllerBase
    {
        private readonly IRepositoryWrapper _repo = repo;

        [HttpGet]
        [Authorize(Roles = "CompanyAdmin,Formateur")]
        public async Task<IActionResult> GetQuestionnaires()
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null || user.CompanyId == null) return Unauthorized();

            var questionnaires = await _repo.Questionnaire.GetByCompany((int)user.CompanyId);
            return Ok(questionnaires);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "CompanyAdmin,Formateur")]
        public async Task<IActionResult> GetQuestionnaire(int id)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var questionnaire = await _repo.Questionnaire.GetWithQuestions(id);
            if (questionnaire == null) return NotFound();
            if (questionnaire.CompanyId != user.CompanyId) return Forbid();

            return Ok(questionnaire);
        }

        [HttpPost]
        [Authorize(Roles = "CompanyAdmin,Formateur")]
        public async Task<IActionResult> CreateQuestionnaire([FromBody] Questionnaire questionnaireIn)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null || user.CompanyId == null) return Unauthorized();

            questionnaireIn.CompanyId = (int)user.CompanyId;
            questionnaireIn.CreatedById = user.Id;
            questionnaireIn.Status = QuestionnaireStatus.Draft; // Default to draft

            var created = await _repo.Questionnaire.Create(questionnaireIn);
            return Ok(created);
        }

        [HttpPost("{id}/questions")]
        [Authorize(Roles = "CompanyAdmin,Formateur")]
        public async Task<IActionResult> AddQuestions(int id, [FromBody] List<QuestionnaireQuestion> newQuestions)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var questionnaire = await _repo.Questionnaire.GetWithQuestions(id);
            if (questionnaire == null) return NotFound("Questionnaire not found.");
            if (questionnaire.CompanyId != user.CompanyId) return Forbid();

            if (questionnaire.Status == QuestionnaireStatus.Published)
            {
                return BadRequest("Cannot modify a published questionnaire.");
            }

            foreach (var nq in newQuestions)
            {
                var q = await _repo.Question.Get(nq.QuestionId);
                if (q == null || q.CompanyId != user.CompanyId) return BadRequest($"Invalid question {nq.QuestionId}");

                nq.QuestionnaireId = id;
                // Avoid using raw repo context so we just add to collection and update parent
                questionnaire.QuestionnaireQuestions.Add(nq);
            }

            var updated = await _repo.Questionnaire.Update(questionnaire);
            return Ok(updated);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "CompanyAdmin,Formateur")]
        public async Task<IActionResult> UpdateQuestionnaire(int id, [FromBody] UpdateQuestionnaireDto questionnaireIn)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(questionnaireIn.Title))
            {
                return BadRequest("Questionnaire title is required.");
            }

            var questionnaire = await _repo.Questionnaire.GetWithQuestions(id);
            if (questionnaire == null) return NotFound();
            if (questionnaire.CompanyId != user.CompanyId) return Forbid();

            if (questionnaire.Status == QuestionnaireStatus.Published && questionnaire.CampaignQuestionnaires.Any())
            {
                return BadRequest("Cannot edit a published questionnaire that is attached to campaigns.");
            }

            questionnaire.Title = questionnaireIn.Title.Trim();
            questionnaire.Description = string.IsNullOrWhiteSpace(questionnaireIn.Description)
                ? null
                : questionnaireIn.Description.Trim();

            var updated = await _repo.Questionnaire.Update(questionnaire);
            return Ok(updated);
        }

        [HttpPut("{id}/publish")]
        [Authorize(Roles = "CompanyAdmin,Formateur")]
        public async Task<IActionResult> PublishQuestionnaire(int id)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var questionnaire = await _repo.Questionnaire.GetWithQuestions(id);
            if (questionnaire == null) return NotFound();
            if (questionnaire.CompanyId != user.CompanyId) return Forbid();

            if (questionnaire.QuestionnaireQuestions.Count == 0)
            {
                return BadRequest("Must have at least 1 question to publish.");
            }

            questionnaire.Status = QuestionnaireStatus.Published;
            var updated = await _repo.Questionnaire.Update(questionnaire);
            return Ok(updated);
        }

        [HttpDelete("{id}/questions/{questionId}")]
        [Authorize(Roles = "CompanyAdmin,Formateur")]
        public async Task<IActionResult> RemoveQuestion(int id, int questionId)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var questionnaire = await _repo.Questionnaire.GetWithQuestions(id);
            if (questionnaire == null) return NotFound();
            if (questionnaire.CompanyId != user.CompanyId) return Forbid();

            if (questionnaire.Status == QuestionnaireStatus.Published)
                return BadRequest("Cannot modify a published questionnaire.");

            var qq = questionnaire.QuestionnaireQuestions.FirstOrDefault(q => q.QuestionId == questionId);
            if (qq == null) return NotFound("Question not in questionnaire.");

            questionnaire.QuestionnaireQuestions.Remove(qq);

            // Re-number orders to stay contiguous
            var reordered = questionnaire.QuestionnaireQuestions.OrderBy(q => q.Order).ToList();
            for (int i = 0; i < reordered.Count; i++) reordered[i].Order = i + 1;

            var updated = await _repo.Questionnaire.Update(questionnaire);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "CompanyAdmin,Formateur")]
        public async Task<IActionResult> DeleteQuestionnaire(int id)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var questionnaire = await _repo.Questionnaire.GetWithQuestions(id);
            if (questionnaire == null) return NotFound();
            if (questionnaire.CompanyId != user.CompanyId) return Forbid();

            if (questionnaire.CampaignQuestionnaires.Any())
            {
                return BadRequest("Cannot delete a questionnaire that is attached to campaigns.");
            }

            await _repo.Questionnaire.Delete(id);
            return Ok();
        }
    }
}
