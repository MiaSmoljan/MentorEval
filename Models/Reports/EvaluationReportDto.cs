namespace MentorEval.Models.Reports
{
    public class EvaluationReportDto
    {
        public int EvaluationId { get; set; }
        public string Title { get; set; } = "";
        public string CourseName { get; set; } = "";
        public DateTime GeneratedAt { get; set; }

        public int RespondentCount { get; set; }
        public int EnrolledCount { get; set; }
        public double? ResponseRate { get; set; } // 0-1

        public double? OverallAverage { get; set; } // Scale10 only
        public string? PrivacyNotice { get; set; }  // if below threshold

        public List<QuestionReportDto> Questions { get; set; } = new();
        public List<string> Comments { get; set; } = new();
        public List<KeywordCountDto> TopKeywords { get; set; } = new();

    }
}
