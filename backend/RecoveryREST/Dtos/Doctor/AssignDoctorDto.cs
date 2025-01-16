using System.ComponentModel.DataAnnotations;

namespace RecoveryREST.Dtos.Doctor {
    public class AssignDoctorDto {
        [Required] public string AppUserId { get; set; } = string.Empty;
        [Required] public int InjuryId { get; set; }
    }
}