using System.ComponentModel.DataAnnotations;

namespace api.dtos.request
{
    public class UpdateQuestionnaireDto
    {
        [Required]
        public required string Title { get; set; }

        public string? Description { get; set; }
    }
}
