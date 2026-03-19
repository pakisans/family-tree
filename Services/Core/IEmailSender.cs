using FamilyTree.Services.Email;

namespace FamilyTree.Services.Core;

/// <summary>
/// Low-level email transport. Responsible only for the SMTP connection and delivery.
/// Called exclusively by EmailBackgroundWorker — never by business services directly.
///
/// This abstraction makes it straightforward to swap Brevo SMTP for an API provider
/// (SendGrid, Brevo API, etc.) without touching any business logic.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(EmailJob job, CancellationToken cancellationToken = default);
}
