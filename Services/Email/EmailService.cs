using family_tree.Configuration;
using FamilyTree.Services.Core;

namespace FamilyTree.Services.Email;

/// <summary>
/// Implements IEmailService for use by business services (AuthService, FamilyInvitationService).
///
/// Responsibilities:
///   1. Build the email template (subject + HTML body).
///   2. Wrap the result in an EmailJob.
///   3. Enqueue the job via IEmailQueue.
///
/// This class does NOT send emails. Sending is handled by EmailSender (transport layer)
/// which is called by EmailBackgroundWorker after dequeuing.
///
/// All public methods return immediately — they never block on SMTP.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IEmailQueue _queue;
    private readonly ISystemConfiguration _config;

    public EmailService(IEmailQueue queue, ISystemConfiguration config)
    {
        _queue = queue;
        _config = config;
    }

    public Task SendInvitationEmailAsync(
        string toEmail,
        string familyName,
        string? inviterName,
        string roleName,
        string token)
    {
        string acceptUrl = $"{_config.FrontUrl.TrimEnd('/')}/accept-invitation?token={token}";

        (string subject, string htmlBody) = EmailTemplates.Invitation(
            familyName, inviterName, roleName, acceptUrl);

        _queue.Enqueue(new EmailJob
        {
            Type    = EmailJobType.Invitation,
            ToEmail = toEmail,
            Subject = subject,
            HtmlBody = htmlBody
        });

        return Task.CompletedTask;
    }

    public Task SendWelcomeEmailAsync(string toEmail, string firstName)
    {
        (string subject, string htmlBody) = EmailTemplates.Welcome(firstName);

        _queue.Enqueue(new EmailJob
        {
            Type    = EmailJobType.Welcome,
            ToEmail = toEmail,
            Subject = subject,
            HtmlBody = htmlBody
        });

        return Task.CompletedTask;
    }
}
