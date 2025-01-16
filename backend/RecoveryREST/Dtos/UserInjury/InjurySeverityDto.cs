using System.ComponentModel.DataAnnotations;

namespace RecoveryREST.Dtos.UserInjury {
    public class InjurySeverityDto {
        [Required] public int InjuryId { get; set; }
        public bool IsTooSevere { get; set; } = false;
    }
}