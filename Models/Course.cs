using System;
using System.Collections.Generic;

namespace MentorEval.Models;

public partial class Course
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int Semester { get; set; }

    public int Year { get; set; }

    public int ProfessorId { get; set; }

    public virtual ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();

    public virtual User Professor { get; set; } = null!;
}
