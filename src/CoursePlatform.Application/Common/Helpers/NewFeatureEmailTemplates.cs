using System.Net;

namespace CoursePlatform.Application.Common.Helpers;

public static class NewFeatureEmailTemplates
{
    public static (string Subject, string Html) NewsletterConfirmation(string confirmationUrl)
    {
        var url = WebUtility.HtmlEncode(confirmationUrl);

        return (
            "Potwierdź subskrypcję newslettera — CoursePlatform",
            $"""
            <!doctype html>
            <html lang="pl">
            <body style="margin:0;padding:24px;background:#f1f5f9;font-family:Segoe UI,Arial,sans-serif;color:#1e293b">
              <main style="max-width:560px;margin:0 auto;padding:32px;background:#ffffff;border-radius:16px;border:1px solid #e2e8f0">
                <h2 style="margin:0 0 12px;color:#b45309">Potwierdź subskrypcję</h2>
                <p>Potwierdź swój adres e-mail, aby otrzymywać newsletter CoursePlatform.</p>
                <p style="margin:24px 0"><a href="{url}" style="display:inline-block;padding:12px 24px;background:#b45309;color:#fff;text-decoration:none;border-radius:8px;font-weight:600">Potwierdź subskrypcję</a></p>
                <p style="font-size:13px;color:#64748b">Jeśli nie zapisywałeś(-aś) się do newslettera, zignoruj tę wiadomość.</p>
              </main>
            </body>
            </html>
            """);
    }
}
