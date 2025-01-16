using System.ComponentModel.DataAnnotations;

namespace RecoveryREST.Dtos.Account {
    public class LoginDto {
        public string? Username { get; set; }
        public string? Email { get; set; }
        [Required] public string Password { get; set; } = string.Empty;
    }
}