namespace FamilyTree.Services.Core;

public interface IEmailService
{
    Task SendInvitationEmailAsync(
        string toEmail,
        string familyName,
        string? inviterName,
        string roleName,
        string token);

    Task SendWelcomeEmailAsync(
        string toEmail,
        string firstName);
}
