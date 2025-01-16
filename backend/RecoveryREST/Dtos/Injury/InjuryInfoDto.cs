using RecoveryREST.Dtos.RecoveryExercise;

namespace RecoveryREST.Dtos.Injury {
    public class InjuryInfoDto {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BodyPart { get; set; } = string.Empty;
        public List<RecoveryExerciseDto> RecoveryExercises { get; set; } = new List<RecoveryExerciseDto>();
    }
}