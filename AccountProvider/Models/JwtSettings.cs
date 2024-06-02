namespace AccountProvider.Models
{
    public class JwtSettings
    {
        public string Secret { get; set; } = null!;
        public int TokenExpirationInMinutes { get; set; }
    }
}