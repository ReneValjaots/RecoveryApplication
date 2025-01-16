using System.ComponentModel.DataAnnotations;
using RecoveryREST.Dtos.RecoveryPlan;

namespace RecoveryREST.Dtos.Doctor {
    public class DoctorUpdatePlanDto {
        [MaxLength(40, ErrorMessage = "Recovery Plan name cannot exceed 40 characters.")]
        public string Name { get; set; } = string.Empty;
        public List<WorkoutDoctorCreateDto>? WorkoutDays { get; set; } = new();
    }
}