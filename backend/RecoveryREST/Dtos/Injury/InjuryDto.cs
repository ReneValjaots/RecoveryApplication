namespace RecoveryREST.Dtos.Injury {
    public class InjuryDto {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BodyPart { get; set; } = string.Empty;
    }
}