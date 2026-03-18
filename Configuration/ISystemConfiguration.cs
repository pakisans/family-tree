using FamilyTree.Configuration;

namespace family_tree.Configuration;

public interface ISystemConfiguration
{
    public string FrontUrl { get; set; }
    public string DatabaseConnection { get; }
    public JwtConfiguration Jwt { get; set; }
    public S3Configuration S3 { get; set; }
    public EmailServerConfiguration EmailServer { get; set; }
    public string ApiKey { get; set; }
    public AdminConfiguration Admin { get; set; }
}
