namespace RecoveryREST.Dtos.RecoveryExercise {
    public class CreateRecoveryExerciseDto {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<int>? InjuryIds { get; set; }
    }
}