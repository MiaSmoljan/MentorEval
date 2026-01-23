using MentorEval.Models;
using MentorEval.Models.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MentorEval.Services
{
    public class PdfService
    {

        public byte[] GenerateEvaluationAnalyticsReport(EvaluationReportDto report)
         {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);

                page.Content().Column(col =>
                {
                    col.Item().Text($"Izvještaj evaluacije").FontSize(18).Bold();
                    col.Item().Text($"{report.CourseName} - {report.Title}");
                    col.Item().Text($"Generirano: {report.GeneratedAt:dd.MM.yyyy HH:mm}");
                    col.Item().Text($"Odgovori: {report.RespondentCount} / {report.EnrolledCount}  (RR: {(report.ResponseRate.HasValue ? (report.ResponseRate.Value * 100).ToString("0.0") : "N/A")}%)");

                    col.Item().PaddingTop(10).Text($"Overall prosjek: {(report.OverallAverage?.ToString("0.00") ?? "N/A")}").Bold();

                    col.Item().PaddingTop(15).Text("Pitanja (Scale10)").FontSize(14).Bold();

                    foreach (var q in report.Questions.Where(x => x.Type == "Scale10"))
                    {
                        col.Item().PaddingTop(8).Text(q.Text).Bold();
                        col.Item().Text($"N={q.AnswerCount}, Avg={(q.Average?.ToString("0.00") ?? "N/A")}, Med={(q.Median?.ToString("0.00") ?? "N/A")}, Min={q.Min?.ToString() ?? "N/A"}, Max={q.Max?.ToString() ?? "N/A"}");
                    }

                    if (report.Comments.Count > 0)
                    {
                        col.Item().PaddingTop(15).Text("Komentari").FontSize(14).Bold();
                        foreach (var c in report.Comments.Take(20))
                            col.Item().Text("- " + c);
                    }

                    if (report.TopKeywords.Count > 0)
                    {
                        col.Item().PaddingTop(15).Text("Top keywords").FontSize(14).Bold();
                        foreach (var k in report.TopKeywords)
                            col.Item().Text($"{k.Keyword} ({k.Count})");
                    }
                });
            });
        }).GeneratePdf();
    }

    public byte[] GenerateProfessorEvaluationsReport(List<Evaluation> evaluations)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.PageColor(Colors.White);
                    page.Content().Column(col =>
                    {
                        col.Item().Text("Professor evaluations report")
                            .FontSize(20).Bold();

                        col.Item().Text($"Generated at: {DateTime.Now:g}")
                            .FontSize(10).FontColor(Colors.Grey.Darken2);

                        col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        if (evaluations == null || !evaluations.Any())
                        {
                            col.Item().Text("No evaluations found.")
                                .FontSize(12);
                        }
                        else
                        {
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Evaluation").SemiBold();
                                    header.Cell().Text("Course").SemiBold();
                                    header.Cell().Text("Time period").SemiBold();
                                    header.Cell().Text("Status").SemiBold();
                                });

                                foreach (var e in evaluations)
                                {
                                    table.Cell().Text(e.Title);
                                    table.Cell().Text(e.Course?.Name ?? "");
                                    table.Cell().Text($"{e.StartAt:d} - {e.EndAt:d}");
                                    table.Cell().Text(e.Status);
                                }
                            });
                        }
                    });
                });
            }).GeneratePdf();
        }
    }
}
