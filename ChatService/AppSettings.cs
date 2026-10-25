namespace ChatService;

public class AppSettings
{
    public string Secret { get; set; } = string.Empty;
    public string[] AllowedChatOrigins { get; set; } = [];
}
