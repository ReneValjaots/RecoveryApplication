namespace RecoveryREST.Models.Classes {
    public class RecoveryPlanExercise {
        public int Id { get; set; }
        public int WorkoutDayId { get; set; }
        public WorkoutDay WorkoutDay { get; set; }
        public int RecoveryExerciseId { get; set; }
        public RecoveryExercise RecoveryExercise { get; set; }
        public int? Sets { get; set; }
        public int? Reps { get; set; }
        public TimeSpan? Duration { get; set; }
    }
}