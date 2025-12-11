using System;
using System.Collections.Generic;

namespace MentorEval.Models;

public partial class Answer
{
    public int Id { get; set; }

    public int? Grade { get; set; }

    public string? Text { get; set; }

    public DateTime SubmittedAt { get; set; }

    public int QuestionId { get; set; }

    public int EvaluationId { get; set; }

    public int StudentId { get; set; }

    public virtual Evaluation Evaluation { get; set; } = null!;

    public virtual Question Question { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}
