using System;
using System.ComponentModel.DataAnnotations;

namespace FloresMvc.Models
{
    public class PasswordResetToken
    {
        [Key]
        public int IdToken { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(128)]
        public string TokenHash { get; set; } = string.Empty;

        [Required]
        public DateTime ExpiresAt { get; set; }

        public DateTime? UsedAt { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
