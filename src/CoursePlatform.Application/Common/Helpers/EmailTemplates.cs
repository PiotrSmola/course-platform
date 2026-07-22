using System.Net;

namespace CoursePlatform.Application.Common.Helpers;

public static class EmailTemplates
{
    private const string Accent = "#b45309";

    // Values are HTML-encoded at the sink so email safety never depends on upstream input validation.
    public static (string Subject, string Html) Welcome(string firstName)
    {
        var name = WebUtility.HtmlEncode(firstName);
        return ("Witaj w CoursePlatform!",
            Wrap($"""
                <h2 style="margin:0 0 12px">Cześć {name}!</h2>
                <p>Twoje konto w <strong>CoursePlatform</strong> jest gotowe. Przeglądaj katalog kursów,
                zapisuj się na darmowe szkolenia i rozwijaj swoje umiejętności.</p>
                <p>Miłej nauki!</p>
                """));
    }

    public static (string Subject, string Html) PurchaseConfirmed(string firstName, string courseTitle, decimal amount, string currency)
    {
        var name = WebUtility.HtmlEncode(firstName);
        var title = WebUtility.HtmlEncode(courseTitle);
        var curr = WebUtility.HtmlEncode(currency.ToUpperInvariant());
        return ($"Potwierdzenie zakupu — {courseTitle}",
            Wrap($"""
                <h2 style="margin:0 0 12px">Dziękujemy za zakup, {name}!</h2>
                <p>Twoja płatność za kurs <strong>{title}</strong> została zrealizowana.</p>
                <p style="font-size:18px"><strong>Kwota: {amount:0.00} {curr}</strong></p>
                <p>Kurs znajdziesz w sekcji „Moje kursy”. Możesz zacząć naukę od razu.</p>
                """));
    }

    public static (string Subject, string Html) CertificateIssued(string firstName, string courseTitle, string certificateNumber)
    {
        var name = WebUtility.HtmlEncode(firstName);
        var title = WebUtility.HtmlEncode(courseTitle);
        var number = WebUtility.HtmlEncode(certificateNumber);
        return ($"Certyfikat ukończenia — {courseTitle}",
            Wrap($"""
                <h2 style="margin:0 0 12px">Gratulacje, {name}!</h2>
                <p>Ukończyłeś(-aś) kurs <strong>{title}</strong> w 100%.</p>
                <p>Twój certyfikat o numerze <strong style="color:{Accent}">{number}</strong>
                czeka w Twoim profilu — możesz go pobrać jako PDF i udostępnić do weryfikacji.</p>
                """));
    }

    public static (string Subject, string Html) PasswordReset(string firstName, string resetUrl)
    {
        var name = WebUtility.HtmlEncode(firstName);
        var url = WebUtility.HtmlEncode(resetUrl);
        return ("Reset hasła — CoursePlatform",
            Wrap($"""
                <h2 style="margin:0 0 12px">Cześć {name}!</h2>
                <p>Otrzymaliśmy prośbę o zresetowanie hasła do Twojego konta.</p>
                <p style="margin:24px 0">
                  <a href="{url}" style="display:inline-block;padding:12px 24px;background:{Accent};color:#fff;text-decoration:none;border-radius:8px;font-weight:600">
                    Ustaw nowe hasło
                  </a>
                </p>
                <p style="font-size:13px;color:#64748b">Link jest ważny przez ograniczony czas. Jeśli to nie Ty — zignoruj tę wiadomość.</p>
                """));
    }

    public static (string Subject, string Html) CoursePublishedWaitlist(string firstName, string courseTitle, string courseUrl)
    {
        var name = WebUtility.HtmlEncode(firstName);
        var title = WebUtility.HtmlEncode(courseTitle);
        var url = WebUtility.HtmlEncode(courseUrl);
        return ($"Kurs dostępny — {courseTitle}",
            Wrap($"""
                <h2 style="margin:0 0 12px">Cześć {name}!</h2>
                <p>Kurs <strong>{title}</strong>, na który czekałeś(-aś), właśnie został opublikowany.</p>
                <p style="margin:24px 0">
                  <a href="{url}" style="display:inline-block;padding:12px 24px;background:{Accent};color:#fff;text-decoration:none;border-radius:8px;font-weight:600">
                    Zobacz kurs
                  </a>
                </p>
                """));
    }

    public static (string Subject, string Html) ConfirmEmail(string firstName, string confirmUrl)
    {
        var name = WebUtility.HtmlEncode(firstName);
        var url = WebUtility.HtmlEncode(confirmUrl);
        return ("Potwierdź adres email — CoursePlatform",
            Wrap($"""
                <h2 style="margin:0 0 12px">Cześć {name}!</h2>
                <p>Potwierdź adres email, aby dokończyć rejestrację w CoursePlatform.</p>
                <p style="margin:24px 0">
                  <a href="{url}" style="display:inline-block;padding:12px 24px;background:{Accent};color:#fff;text-decoration:none;border-radius:8px;font-weight:600">
                    Potwierdź email
                  </a>
                </p>
                <p style="font-size:13px;color:#64748b">Jeśli nie zakładałeś(-aś) konta — zignoruj tę wiadomość.</p>
                """));
    }

    private static string Wrap(string content) =>
        $"""
        <!doctype html>
        <html lang="pl">
        <body style="margin:0;padding:0;background:#f1f5f9;font-family:Segoe UI,Arial,sans-serif;color:#1e293b">
          <div style="max-width:560px;margin:24px auto;padding:32px;background:#ffffff;border-radius:16px;border:1px solid #e2e8f0">
            <p style="margin:0 0 20px;font-size:20px;font-weight:700;color:{Accent}">CoursePlatform</p>
            {content}
            <hr style="border:none;border-top:1px solid #e2e8f0;margin:24px 0">
            <p style="font-size:12px;color:#94a3b8;margin:0">
              Ta wiadomość została wysłana automatycznie — prosimy na nią nie odpowiadać.
            </p>
          </div>
        </body>
        </html>
        """;
}
