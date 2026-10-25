namespace AuthService;

public class AppSettings
{
    public string Secret { get; set; } = string.Empty;
    public string[] AllowedAuthOrigins { get; set; } = [];
}
