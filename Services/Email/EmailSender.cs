using family_tree.Configuration;
using FamilyTree.Services.Core;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace FamilyTree.Services.Email;

/// <summary>
/// Brevo SMTP implementation of IEmailSender.
/// Responsible solely for the transport layer — connecting, authenticating, and sending.
///
/// EnableEmailSending = false (e.g. in local dev) causes the email to be logged
/// but not actually sent. All other behaviour is unchanged.
///
/// This class is registered as Singleton because it holds no per-request state.
/// </summary>
public class EmailSender : IEmailSender
{
    private readonly ISystemConfiguration _config;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(ISystemConfiguration config, ILogger<EmailSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(EmailJob job, CancellationToken cancellationToken = default)
    {
        EmailServerConfiguration cfg = _config.EmailServer;

        if (!cfg.EnableEmailSending)
        {
            _logger.LogInformation(
                "[EMAIL DISABLED] Type={Type} To={ToEmail} Subject={Subject} — " +
                "Set EmailServer:EnableEmailSending=true to enable real delivery.",
                job.Type, job.ToEmail, job.Subject);
            return;
        }

        if (string.IsNullOrWhiteSpace(cfg.Host) ||
            string.IsNullOrWhiteSpace(cfg.Username) ||
            string.IsNullOrWhiteSpace(cfg.Password) ||
            string.IsNullOrWhiteSpace(cfg.FromEmail))
        {
            _logger.LogWarning(
                "[EMAIL NOT CONFIGURED] Type={Type} To={ToEmail} — " +
                "Set EmailServer:Host, Username, Password, FromEmail.",
                job.Type, job.ToEmail);
            return;
        }

        MimeMessage message = BuildMessage(job);

        using SmtpClient client = new SmtpClient();

        // CRL (certificate revocation list) checks frequently fail on macOS and Linux
        // due to network restrictions preventing access to Brevo's CRL distribution points.
        // The certificate itself is still fully validated — only the revocation check is skipped.
        client.CheckCertificateRevocation = false;

        await client.ConnectAsync(cfg.Host, cfg.Port, SecureSocketOptions.StartTls, cancellationToken);
        await client.AuthenticateAsync(cfg.Username, cfg.Password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation(
            "Email delivered. Type={Type} To={ToEmail} Subject={Subject}",
            job.Type, job.ToEmail, job.Subject);
    }

    private MimeMessage BuildMessage(EmailJob job)
    {
        EmailServerConfiguration cfg = _config.EmailServer;

        MimeMessage message = new MimeMessage();
        message.From.Add(new MailboxAddress(cfg.FromName, cfg.FromEmail));
        message.To.Add(MailboxAddress.Parse(job.ToEmail));
        message.Subject = job.Subject;
        message.Body = new BodyBuilder { HtmlBody = job.HtmlBody }.ToMessageBody();

        return message;
    }
}
