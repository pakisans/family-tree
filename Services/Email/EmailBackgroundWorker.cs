using FamilyTree.Services.Core;

namespace FamilyTree.Services.Email;

/// <summary>
/// Long-running background service that drains the EmailQueue and delivers emails.
///
/// Retry policy:
///   Attempt 1 failure → wait 2s  → retry
///   Attempt 2 failure → wait 8s  → retry
///   Attempt 3 failure → wait 30s → retry
///   Attempt 4 failure → permanent failure, email is discarded
///
/// The delayed re-enqueue runs on the thread pool so the worker loop is
/// never blocked by a backoff delay — other queued emails continue to flow.
///
/// On app shutdown (stoppingToken cancelled), in-flight delayed retries are
/// abandoned. Emails that have not yet been dequeued are lost (in-memory queue).
/// For durable retries, replace IEmailQueue with a persistent queue.
/// </summary>
public class EmailBackgroundWorker : BackgroundService
{
    private const int MaxRetries = 3;
    private static readonly int[] RetryDelaysSeconds = [2, 8, 30];

    private readonly IEmailQueue _queue;
    private readonly IEmailSender _sender;
    private readonly ILogger<EmailBackgroundWorker> _logger;

    public EmailBackgroundWorker(
        IEmailQueue queue,
        IEmailSender sender,
        ILogger<EmailBackgroundWorker> logger)
    {
        _queue = queue;
        _sender = sender;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailBackgroundWorker started.");

        await foreach (EmailJob job in _queue.ReadAllAsync(stoppingToken))
        {
            await ProcessJobAsync(job, stoppingToken);
        }

        _logger.LogInformation("EmailBackgroundWorker stopped.");
    }

    private async Task ProcessJobAsync(EmailJob job, CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogDebug(
                "Sending email. Type={Type} To={ToEmail} Attempt={Attempt}",
                job.Type, job.ToEmail, job.RetryCount + 1);

            await _sender.SendAsync(job, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // App is shutting down — do not retry.
            _logger.LogWarning(
                "Email send cancelled during shutdown. Type={Type} To={ToEmail}",
                job.Type, job.ToEmail);
        }
        catch (Exception ex)
        {
            if (job.RetryCount < MaxRetries)
            {
                job.RetryCount++;
                int delaySeconds = RetryDelaysSeconds[job.RetryCount - 1];

                _logger.LogWarning(ex,
                    "Email send failed (attempt {Attempt}/{Max}). " +
                    "Type={Type} To={ToEmail} — retrying in {Delay}s.",
                    job.RetryCount, MaxRetries + 1,
                    job.Type, job.ToEmail, delaySeconds);

                // Schedule re-enqueue on thread pool after backoff delay.
                // The worker loop is not blocked and continues draining the queue.
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
                        _queue.Enqueue(job);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogWarning(
                            "Email retry cancelled during shutdown. " +
                            "Type={Type} To={ToEmail} Attempt={Attempt}",
                            job.Type, job.ToEmail, job.RetryCount);
                    }
                }, stoppingToken);
            }
            else
            {
                _logger.LogError(ex,
                    "Email permanently failed after {Max} attempts and will not be retried. " +
                    "Type={Type} To={ToEmail} Subject={Subject} EnqueuedAt={EnqueuedAt}",
                    MaxRetries + 1,
                    job.Type, job.ToEmail, job.Subject, job.EnqueuedAt);
            }
        }
    }
}
