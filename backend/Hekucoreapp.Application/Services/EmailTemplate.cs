using Microsoft.Extensions.Localization;
using Hekucoreapp.Application.Resources;

namespace Hekucoreapp.Application.Services;

public class EmailTemplates
{
    private readonly IStringLocalizer<Messages> _localizer;

    public EmailTemplates(IStringLocalizer<Messages> localizer)
    {
        _localizer = localizer;
    }

    public (string Subject, string Body) Welcome(string fullName)
    {
        var subject = _localizer["EmailWelcomeSubject"];
        var title = string.Format(_localizer["EmailWelcomeTitle"], fullName);
        var body = _localizer["EmailWelcomeBody"];

        var html = $@"
            <h2>{title}</h2>
            <p>{body}</p>
        ";

        return (subject, html);
    }

    public (string Subject, string Body) PasswordChanged(string fullName)
    {
        var subject = _localizer["EmailPasswordChangedSubject"];
        var title = _localizer["EmailPasswordChangedTitle"];
        var body = string.Format(_localizer["EmailPasswordChangedBody"], fullName);

        var html = $@"
            <h2>{title}</h2>
            <p>{body}</p>
        ";

        return (subject, html);
    }

    public (string Subject, string Body) PasswordReset(string fullName, string temporaryPassword)
    {
        var subject = _localizer["EmailPasswordResetSubject"];
        var title = _localizer["EmailPasswordResetTitle"];
        var body = string.Format(_localizer["EmailPasswordResetBody"], fullName);
        var footer = _localizer["EmailPasswordResetFooter"];

        var html = $@"
            <h2>{title}</h2>
            <p>{body}</p>
            <p style=""font-size: 18px; font-weight: bold;"">{temporaryPassword}</p>
            <p>{footer}</p>
        ";

        return (subject, html);
    }
}