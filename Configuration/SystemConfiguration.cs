namespace family_tree.Configuration;

public class SystemConfiguration : ISystemConfiguration
{
    public string FrontUrl { get; set; } = string.Empty;
    public string DatabaseConnection { get; set; } = string.Empty;
    public JwtConfiguration Jwt { get; set; } = new();
    public S3Configuration S3 { get; set; } = new();
    public EmailServerConfiguration EmailServer { get; set; } = new();
    public string ApiKey { get; set; } = string.Empty;
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
}
