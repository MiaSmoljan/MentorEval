using System.ComponentModel.DataAnnotations;

namespace MentorEval.Models
{
    public class EvaluationCreateViewModel
    {
        [Required]
        public int CourseId { get; set; }

        public string? CourseName { get; set; }

        [Required]
        public string Title { get; set; } = "";

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartAt { get; set; } = DateTime.Today;

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndAt { get; set; } = DateTime.Today.AddDays(7);

        public List<QuestionCreateViewModel> Questions { get; set; } = new();
        public List<Course> AvailableCourses { get; set; } = new(); 
    }

    public class QuestionCreateViewModel
    {
        [Required]
        public string Text { get; set; } = "";

        [Required]
        public string Type { get; set; } = "Scale10"; 

        public bool Required { get; set; } = true;
    }
}
