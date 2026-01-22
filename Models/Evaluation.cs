using System;
using System.Collections.Generic;

namespace MentorEval.Models;

public partial class Evaluation
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public string Status { get; set; } = null!;

    public int CourseId { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual Report? Report { get; set; }

    public virtual ICollection<User> Students { get; set; } = new List<User>();
}
