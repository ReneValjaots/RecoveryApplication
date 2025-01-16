namespace RecoveryREST.Dtos.RecoveryPlan {
    public class WorkoutDoctorCreateDto {
        public int DayNumber { get; set; }
        public List<ExerciseDto>? Exercises { get; set; } = new();
    }
}