namespace RecoveryREST.Models.Classes {
    public class WorkoutDay {
        public int Id { get; set; }
        public int DayNumber { get; set; }
        public int RecoveryPlanId { get; set; }
        public RecoveryPlan RecoveryPlan { get; set; }
        public List<RecoveryPlanExercise> RecoveryPlanExercises { get; set; } = new List<RecoveryPlanExercise>();
    }
}