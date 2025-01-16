namespace RecoveryREST.Dtos.RecoveryPlan {
    public class CreateRecoveryPlanDto {
        public string Name { get; set; } = string.Empty;
        public string AppUserId { get; set; } = string.Empty;
        public List<WorkoutDoctorCreateDto>? WorkoutDays { get; set; } = new();
    }
}