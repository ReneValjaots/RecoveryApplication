namespace RecoveryREST.Dtos.RecoveryPlan {
    public class WorkoutDayDto {
        public int DayNumber { get; set; }
        public List<RecoveryExerciseDetailDto> Exercises { get; set; } = new List<RecoveryExerciseDetailDto>();
    }
}