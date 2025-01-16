namespace RecoveryREST.Models.Classes {
    public class InjuryRecoveryExercise {
        public int InjuryId { get; set; }
        public Injury Injury { get; set; }

        public int RecoveryExerciseId { get; set; }
        public RecoveryExercise RecoveryExercise { get; set; }
    }
}