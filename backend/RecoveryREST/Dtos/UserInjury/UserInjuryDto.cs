using RecoveryREST.Dtos.RecoveryExercise;

namespace RecoveryREST.Dtos.UserInjury {
    public class UserInjuryDto {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AppUserId { get; set; } = string.Empty;
        public bool IsTooSevere { get; set; } = false;
        public List<RecoveryExerciseDto> RecoveryExercises { get; set; } = new List<RecoveryExerciseDto>();
    }
}