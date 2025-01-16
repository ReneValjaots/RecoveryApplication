using System.ComponentModel.DataAnnotations;

namespace RecoveryREST.Dtos.UserInjury {
    public class AssignedInjuryDto {
        [Required] public int InjuryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsTooSevere { get; set; }
    }
}