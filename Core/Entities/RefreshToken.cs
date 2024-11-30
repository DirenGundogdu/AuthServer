namespace Core.Entities;

public class RefreshToken
{
    public string UserId { get; set; }
    public string Code { get; set; }
    public DateTime Expiration { get; set; }
}