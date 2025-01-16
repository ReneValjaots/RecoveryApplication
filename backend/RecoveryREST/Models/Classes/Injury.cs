namespace RecoveryREST.Models.Classes {
    public class Injury {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BodyPart { get; set; } = string.Empty;
        public List<InjuryRecoveryExercise> InjuryRecoveryExercises { get; set; } = new List<InjuryRecoveryExercise>();
        public List<UserInjury> UserInjuries { get; set; } = new List<UserInjury>();
    }
}