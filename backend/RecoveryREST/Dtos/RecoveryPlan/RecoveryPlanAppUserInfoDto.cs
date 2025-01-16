namespace RecoveryREST.Dtos.RecoveryPlan {
    public class RecoveryPlanAppUserInfoDto {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<WorkoutDayDto> WorkoutDays { get; set; } = new List<WorkoutDayDto>();
        public string? AppUserId { get; set; }
    }
}