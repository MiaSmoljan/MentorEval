using System.ComponentModel.DataAnnotations;

namespace MentorEval.Models;

public class StudentEvaluationFillViewModel
{
    public int EvaluationId { get; set; }
    public string Title { get; set; } = "";
    public string CourseName { get; set; } = "";
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    public List<StudentQuestionAnswerVM> Questions { get; set; } = new();
}

public class StudentQuestionAnswerVM
{
    public int QuestionId { get; set; }
    public string Text { get; set; } = "";
    public string Type { get; set; } = "";   // "Scale10" ili "Text"
    public bool Required { get; set; }

    // Studentov unos:
    [Range(1, 10, ErrorMessage = "Ocjena mora biti između 1 i 10.")]
    public int? Grade { get; set; }

    [MaxLength(2000)]
    public string? AnswerText { get; set; }
}
