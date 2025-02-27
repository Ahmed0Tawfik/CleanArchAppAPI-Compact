namespace CleanArch.Application.Models.Auth
{
    public class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public long ExpiresIn { get; set; }
    }
}
