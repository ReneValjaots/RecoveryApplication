namespace RecoveryREST.Dtos.RecoveryPlan {
    public class RecoveryPlanInfoDto {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<WorkoutDayDto> WorkoutDays { get; set; } = new List<WorkoutDayDto>();
        public bool IsCreatedByDoctor { get; set; }
    }
}