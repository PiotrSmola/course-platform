using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Infrastructure.Options;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CoursePlatform.Infrastructure.Services;

internal sealed class QuestPdfCertificateGenerator : ICertificatePdfGenerator
{
    private const string Ink = "#1e293b";
    private const string Muted = "#64748b";
    private const string Gold = "#b45309";
    private const string Border = "#e2e8f0";

    private readonly CertificateOptions _options;

    public QuestPdfCertificateGenerator(IOptions<CertificateOptions> options)
    {
        _options = options.Value;
    }

    public byte[] Generate(CertificateData data)
    {
        var verificationUrl = $"{_options.VerificationBaseUrl.TrimEnd('/')}/{data.Number}";

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(36);
                page.DefaultTextStyle(style => style.FontFamily(Fonts.Verdana).FontColor(Ink));

                page.Content()
                    .Border(2)
                    .BorderColor(Gold)
                    .Padding(6)
                    .Border(1)
                    .BorderColor(Border)
                    .Padding(40)
                    .Column(column =>
                    {
                        column.Spacing(14);

                        column.Item().AlignCenter().Text("CoursePlatform")
                            .FontSize(20).Bold().FontColor(Gold);

                        column.Item().AlignCenter().Text("CERTYFIKAT UKOŃCZENIA KURSU")
                            .FontSize(30).Bold().LetterSpacing(0.05f);

                        column.Item().PaddingTop(10).AlignCenter().Text("Niniejszym zaświadcza się, że")
                            .FontSize(13).FontColor(Muted);

                        column.Item().AlignCenter().Text(data.HolderName)
                            .FontSize(34).Bold();

                        column.Item().AlignCenter().Text("ukończył(a) kurs")
                            .FontSize(13).FontColor(Muted);

                        column.Item().AlignCenter().Text(data.CourseTitle)
                            .FontSize(22).SemiBold().FontColor(Gold);

                        column.Item().PaddingTop(24).Row(row =>
                        {
                            row.RelativeItem().Column(left =>
                            {
                                left.Item().Text($"Data wystawienia: {data.IssuedAt:dd.MM.yyyy}")
                                    .FontSize(11).FontColor(Muted);
                                left.Item().Text($"Numer certyfikatu: {data.Number}")
                                    .FontSize(11).FontColor(Muted);
                                left.Item().Text($"Weryfikacja: {verificationUrl}")
                                    .FontSize(9).FontColor(Muted);
                            });

                            row.ConstantItem(220).Column(right =>
                            {
                                right.Item().AlignCenter().PaddingBottom(4)
                                    .Text(data.InstructorName).FontSize(13).SemiBold();
                                right.Item().BorderTop(1).BorderColor(Ink).PaddingTop(4)
                                    .AlignCenter().Text("Instruktor kursu").FontSize(10).FontColor(Muted);
                            });
                        });
                    });
            });
        });

        return document.GeneratePdf();
    }
}
