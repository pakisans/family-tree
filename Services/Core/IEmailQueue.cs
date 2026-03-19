using FamilyTree.Services.Email;

namespace FamilyTree.Services.Core;

/// <summary>
/// Thread-safe queue for email jobs.
/// Business services call Enqueue() — they never block on SMTP.
/// The EmailBackgroundWorker consumes via ReadAllAsync().
/// </summary>
public interface IEmailQueue
{
    void Enqueue(EmailJob job);
    IAsyncEnumerable<EmailJob> ReadAllAsync(CancellationToken cancellationToken);
}
