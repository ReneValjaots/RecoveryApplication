namespace RecoveryREST.Dtos.Injury {
    public class CreateInjuryDto {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BodyPart { get; set; } = string.Empty;
        public List<int>? RecoveryExerciseIds { get; set; }
    }
}