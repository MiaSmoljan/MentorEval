using System;
using System.Collections.Generic;

namespace MentorEval.Models;

public partial class Question
{
    public int Id { get; set; }

    public string Text { get; set; } = null!;

    public string Type { get; set; } = null!;

    public bool Required { get; set; }

    public int EvaluationId { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual Evaluation Evaluation { get; set; } = null!;
}
