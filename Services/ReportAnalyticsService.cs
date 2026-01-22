using MentorEval.Models;
using MentorEval.Models.Reports;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MentorEval.Services;

public class ReportAnalyticsService
{
    private readonly AppDbContext _db;
    private const int PrivacyThreshold = 1;

    public ReportAnalyticsService(AppDbContext db) => _db = db;

    public async Task<EvaluationReportDto?> BuildEvaluationReportAsync(int evaluationId, int professorId)
    {
        var eval = await _db.Evaluations
            .Include(e => e.Course)
            .Include(e => e.Students)
            .Include(e => e.Questions)
                .ThenInclude(q => q.Answers)
            .Include(e => e.Answers)
            .FirstOrDefaultAsync(e => e.Id == evaluationId && e.Course.ProfessorId == professorId);

        if (eval == null) return null;

        // StudentId je int -> nema HasValue / Value
        int respondentCount = eval.Answers
            .Select(a => a.StudentId)
            .Distinct()
            .Count();

        int enrolledCount = eval.Students?.Count ?? 0;

        var dto = new EvaluationReportDto
        {
            EvaluationId = eval.Id,
            Title = eval.Title ?? "",
            CourseName = eval.Course?.Name ?? "",
            GeneratedAt = DateTime.Now,
            RespondentCount = respondentCount,
            EnrolledCount = enrolledCount,
            ResponseRate = enrolledCount > 0 ? (double)respondentCount / enrolledCount : null
        };

        // zaštita anonimnosti
        if (respondentCount < PrivacyThreshold)
        {
            dto.PrivacyNotice =
                $"Premalo odgovora za detaljnu analizu (min. {PrivacyThreshold}). Prikaz je ograničen radi anonimnosti.";
            return dto;
        }

        var scaleAverages = new List<double>();

        foreach (var q in eval.Questions.OrderBy(x => x.Id))
        {
            var qr = new QuestionReportDto
            {
                QuestionId = q.Id,
                Text = q.Text ?? "",
                Type = q.Type ?? ""
            };

            if (qr.Type == "Scale10")
            {
                var grades = q.Answers
                    .Where(a => a.Grade.HasValue)
                    .Select(a => a.Grade!.Value)
                    .ToList();

                qr.AnswerCount = grades.Count;

                if (grades.Count > 0)
                {
                    grades.Sort();

                    qr.Average = grades.Average();
                    qr.Median = grades.Count % 2 == 1
                        ? grades[grades.Count / 2]
                        : (grades[(grades.Count / 2) - 1] + grades[grades.Count / 2]) / 2.0;

                    qr.Min = grades.First();
                    qr.Max = grades.Last();

                    var dist = Enumerable.Range(1, 10).ToDictionary(x => x, _ => 0);
                    foreach (var g in grades.Where(g => g is >= 1 and <= 10))
                        dist[g]++;

                    qr.Distribution = dist;

                    // za overall score
                    if (qr.Average.HasValue)
                        scaleAverages.Add(qr.Average.Value);
                }
            }
            else if (qr.Type == "Text")
            {
                var comments = q.Answers
                    .Where(a => !string.IsNullOrWhiteSpace(a.Text))
                    .Select(a => a.Text!.Trim())
                    .ToList();

                qr.AnswerCount = comments.Count;
                dto.Comments.AddRange(comments);
            }

            dto.Questions.Add(qr);
        }

        dto.OverallAverage = scaleAverages.Count > 0 ? scaleAverages.Average() : null;
        dto.TopKeywords = ExtractTopKeywords(dto.Comments, top: 10);

        return dto;
    }

    public async Task SaveReportSnapshotAsync(int evaluationId, EvaluationReportDto dto, string type = "Evaluation")
    {
        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });

        var existing = await _db.Reports.FirstOrDefaultAsync(r => r.EvaluationId == evaluationId);

        if (existing == null)
        {
            _db.Reports.Add(new Report
            {
                EvaluationId = evaluationId,
                GeneratedAt = DateTime.Now,
                Type = type,
                JsonSummary = json
            });
        }
        else
        {
            existing.GeneratedAt = DateTime.Now;
            existing.Type = type;
            existing.JsonSummary = json;
        }

        await _db.SaveChangesAsync();
    }

    private static List<KeywordCountDto> ExtractTopKeywords(List<string> comments, int top)
    {
        var stop = new HashSet<string>(new[]
        {
            "i","a","ali","da","je","su","se","na","u","za","s","sam","smo","ste","što","koji","koja","koje",
            "the","and","or","to","is","are","of","in","on","for","with"
        });

        var counts = new Dictionary<string, int>();

        foreach (var c in comments)
        {
            var words = c.ToLowerInvariant()
                .Replace(",", " ").Replace(".", " ").Replace("!", " ").Replace("?", " ")
                .Replace(";", " ").Replace(":", " ").Replace("(", " ").Replace(")", " ")
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var w in words)
            {
                if (w.Length < 4) continue;
                if (stop.Contains(w)) continue;

                counts[w] = counts.TryGetValue(w, out var n) ? n + 1 : 1;
            }
        }

        return counts
            .OrderByDescending(kv => kv.Value)
            .Take(top)
            .Select(kv => new KeywordCountDto { Keyword = kv.Key, Count = kv.Value })
            .ToList();
    }
}
