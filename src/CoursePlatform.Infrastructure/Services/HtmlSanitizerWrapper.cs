using Ganss.Xss;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Infrastructure.Services;

public class HtmlSanitizerWrapper : CoursePlatform.Application.Common.Interfaces.IHtmlSanitizer
{
    private readonly Ganss.Xss.HtmlSanitizer _sanitizer;

    public HtmlSanitizerWrapper()
    {
        _sanitizer = new Ganss.Xss.HtmlSanitizer();
        _sanitizer.AllowedTags.Clear();
        _sanitizer.AllowedTags.Add("p");
        _sanitizer.AllowedTags.Add("br");
        _sanitizer.AllowedTags.Add("strong");
        _sanitizer.AllowedTags.Add("b");
        _sanitizer.AllowedTags.Add("em");
        _sanitizer.AllowedTags.Add("i");
        _sanitizer.AllowedTags.Add("u");
        _sanitizer.AllowedTags.Add("h1");
        _sanitizer.AllowedTags.Add("h2");
        _sanitizer.AllowedTags.Add("h3");
        _sanitizer.AllowedTags.Add("h4");
        _sanitizer.AllowedTags.Add("ul");
        _sanitizer.AllowedTags.Add("ol");
        _sanitizer.AllowedTags.Add("li");
        _sanitizer.AllowedTags.Add("a");
        _sanitizer.AllowedTags.Add("blockquote");
        _sanitizer.AllowedTags.Add("code");
        _sanitizer.AllowedTags.Add("pre");
        _sanitizer.AllowedTags.Add("span");
        _sanitizer.AllowedTags.Add("div");

        _sanitizer.AllowedAttributes.Clear();
        _sanitizer.AllowedAttributes.Add("href");
        _sanitizer.AllowedAttributes.Add("title");
        _sanitizer.AllowedAttributes.Add("class");
        _sanitizer.AllowedAttributes.Add("target");

        _sanitizer.AllowedCssProperties.Clear();
        _sanitizer.AllowedAtRules.Clear();
    }

    public string Sanitize(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return html ?? string.Empty;
        return _sanitizer.Sanitize(html);
    }
}