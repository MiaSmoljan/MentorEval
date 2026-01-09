using MentorEval.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Diagnostics.CodeAnalysis;

namespace MentorEval.Services
{
    [ExcludeFromCodeCoverage]
    public class PdfService
    {
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
