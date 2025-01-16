using System.ComponentModel.DataAnnotations;

namespace RecoveryREST.Dtos.RecoveryPlan {
    public class RecoveryPlanDto {
        public int Id { get; set; }
        [MaxLength(40, ErrorMessage = "Recovery Plan name cannot exceed 40 characters.")]
        public string Name { get; set; } = string.Empty;
        public List<WorkoutDayDto> WorkoutDays { get; set; } = new List<WorkoutDayDto>();
    }
}