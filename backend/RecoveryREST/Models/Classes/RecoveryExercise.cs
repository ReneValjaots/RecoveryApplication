namespace RecoveryREST.Models.Classes {
    public class RecoveryExercise {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<InjuryRecoveryExercise> InjuryRecoveryExercises { get; set; } = [];
        public List<RecoveryPlanExercise> RecoveryPlanExercises { get; set; } = [];
    }
}