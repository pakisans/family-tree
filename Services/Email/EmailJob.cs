namespace FamilyTree.Services.Email;

public enum EmailJobType
{
    Welcome = 1,
    Invitation = 2
}

/// <summary>
/// Represents a single email send task in the queue.
/// The subject and HTML body are pre-built before enqueuing so the background
/// worker has no dependency on template logic.
/// RetryCount is mutable so the worker can increment it on each failed attempt.
/// </summary>
public class EmailJob
{
    public required EmailJobType Type { get; init; }
    public required string ToEmail { get; init; }
    public required string Subject { get; init; }
    public required string HtmlBody { get; init; }
    public int RetryCount { get; set; } = 0;
    public DateTime EnqueuedAt { get; init; } = DateTime.UtcNow;
}
