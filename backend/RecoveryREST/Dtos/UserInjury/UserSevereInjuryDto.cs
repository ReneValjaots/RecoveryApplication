using System.ComponentModel.DataAnnotations;

namespace RecoveryREST.Dtos.UserInjury {
    public class UserSevereInjuryDto {
        public string AppUserId { get; set; } = string.Empty; 
        [Required] public int InjuryId { get; set; }
        public string Username { get; set; } = string.Empty;
    }
}