using System.ComponentModel.DataAnnotations;

namespace CustomerOrderApi.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public string? PasswordResetToken { get; set; } // Token for resetting password
        public DateTime? PasswordResetTokenExpiry { get; set; } // Expiry for token

    }
}

