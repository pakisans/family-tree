using FamilyTree.Configuration;

namespace family_tree.Configuration;

public class SystemConfiguration : ISystemConfiguration
{
    public string FrontUrl { get; set; } = string.Empty;
    public string DatabaseConnection { get; set; } = string.Empty;
    public JwtConfiguration Jwt { get; set; } = new();
    public S3Configuration S3 { get; set; } = new();
    public EmailServerConfiguration EmailServer { get; set; } = new();
    public string ApiKey { get; set; } = string.Empty;
    public AdminConfiguration Admin { get; set; } = new();
}

public class S3Configuration
{
    public string Region { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public string Bucket { get; set; } = string.Empty;
}

public class JwtConfiguration
{
    public string Key { get; set; } = string.Empty;
    public string Authority { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}


public class EmailServerConfiguration
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public string AuthMode { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Rodoslov";

    /// <summary>
    /// When false, emails are logged but not delivered via SMTP.
    /// Set to false in local development to avoid sending real emails.
    /// </summary>
    public bool EnableEmailSending { get; set; } = false;
}
