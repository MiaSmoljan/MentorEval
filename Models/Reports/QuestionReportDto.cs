namespace MentorEval.Models.Reports
{
    public class QuestionReportDto
    {
        public int QuestionId { get; set; }
        public string Text { get; set; } = "";
        public string Type { get; set; } = "";

        public int AnswerCount { get; set; }

        
        public double? Average { get; set; }
        public double? Median { get; set; }
        public int? Min { get; set; }
        public int? Max { get; set; }
        public Dictionary<int, int>? Distribution { get; set; } 
    }
}
