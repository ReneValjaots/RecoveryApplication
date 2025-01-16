using RecoveryREST.Dtos.Injury;

namespace RecoveryREST.Dtos.RecoveryExercise {
    public class RecoveryExerciseInfoDto {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<InjuryDto> Injuries { get; set; } = new List<InjuryDto>();
    }
}