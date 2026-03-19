using System.Threading.Channels;
using FamilyTree.Services.Core;

namespace FamilyTree.Services.Email;

/// <summary>
/// In-memory email queue backed by System.Threading.Channels.
///
/// Registered as Singleton so the same channel instance is shared between
/// the scoped EmailService (writer) and the singleton EmailBackgroundWorker (reader).
///
/// SingleReader = true because only EmailBackgroundWorker reads.
/// AllowSynchronousContinuations = false prevents reader continuations from
/// running on the writer's thread, keeping request threads clean.
/// </summary>
public class EmailQueue : IEmailQueue
{
    private readonly Channel<EmailJob> _channel = Channel.CreateUnbounded<EmailJob>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            AllowSynchronousContinuations = false
        });

    public void Enqueue(EmailJob job) => _channel.Writer.TryWrite(job);

    public IAsyncEnumerable<EmailJob> ReadAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
