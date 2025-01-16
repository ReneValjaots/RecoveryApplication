namespace RecoveryREST.Dtos.RecoveryPlan {
    public class AssignRecoveryExerciseDto {
        public int DayNumber { get; set; }
        public int Sets { get; set; }
        public int Reps { get; set; }
        public TimeSpan? Duration { get; set; }
    }
}