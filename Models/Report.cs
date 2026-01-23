using System;
using System.Collections.Generic;

namespace MentorEval.Models;

public partial class Report
{
    public int Id { get; set; }

    public DateTime GeneratedAt { get; set; }

    public string Type { get; set; } = null!;

    public string? JsonSummary { get; set; }

    public int EvaluationId { get; set; }

    public virtual Evaluation Evaluation { get; set; } = null!;
}

