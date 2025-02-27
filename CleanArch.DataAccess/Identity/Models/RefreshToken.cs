using CleanArch.Domain;

namespace CleanArch.Infrastructure.Identity.Models
{
    public class RefreshToken : BaseEntity
    {
        public string Token { get; set; }
        public string JwtId { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool Used { get; set; }
        public bool Invalidated { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
