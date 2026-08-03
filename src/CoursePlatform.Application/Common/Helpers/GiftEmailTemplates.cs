using System.Net;

namespace CoursePlatform.Application.Common.Helpers;

public static class GiftEmailTemplates
{
    public static (string Subject, string Html) GiftReceived(
        string courseTitle,
        string code,
        string redeemUrl)
    {
        var title = WebUtility.HtmlEncode(courseTitle);
        var giftCode = WebUtility.HtmlEncode(code);
        var url = WebUtility.HtmlEncode(redeemUrl);

        return (
            $"Otrzymujesz kurs w prezencie — {courseTitle}",
            $"""
            <!doctype html>
            <html lang="pl">
            <body style="margin:0;padding:24px;background:#f1f5f9;font-family:Segoe UI,Arial,sans-serif;color:#1e293b">
              <main style="max-width:560px;margin:0 auto;padding:32px;background:#ffffff;border-radius:16px;border:1px solid #e2e8f0">
                <h2 style="margin:0 0 12px;color:#b45309">Masz prezent!</h2>
                <p>Otrzymujesz dostęp do kursu <strong>{title}</strong>.</p>
                <p>Twój kod prezentowy:</p>
                <p style="font-size:20px;font-weight:700;letter-spacing:1px">{giftCode}</p>
                <p style="margin:24px 0"><a href="{url}" style="display:inline-block;padding:12px 24px;background:#b45309;color:#fff;text-decoration:none;border-radius:8px;font-weight:600">Zrealizuj kod</a></p>
              </main>
            </body>
            </html>
            """);
    }
}
