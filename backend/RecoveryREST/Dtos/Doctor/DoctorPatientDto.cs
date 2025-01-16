namespace RecoveryREST.Dtos.Doctor {
    public class DoctorPatientDto {
        public string AppUserId { get; set; } = string.Empty;
        public int InjuryId { get; set; }
        public bool IsTooSevere { get; set; }
        public string Username { get; set; } = string.Empty;
    }
}