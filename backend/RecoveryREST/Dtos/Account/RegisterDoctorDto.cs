using System.ComponentModel.DataAnnotations;

namespace RecoveryREST.Dtos.Account {
    public class RegisterDoctorDto {
        [Required] public string? Username { get; set; }
        [Required][EmailAddress] public string? Email { get; set; }
        [Required] public string? Password { get; set; }
        [Required] public string SecretKey { get; set; } = string.Empty;
    }
}