namespace RecoveryREST.Dtos.RecoveryPlan {
    public class ExerciseDto {
        public int Id { get; set; }
        public int? Sets { get; set; }
        public int? Reps { get; set; }
        public TimeSpan? Duration { get; set; }
    }
}