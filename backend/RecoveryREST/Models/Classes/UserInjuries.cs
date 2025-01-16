namespace RecoveryREST.Models.Classes {
    public class UserInjury {
        public string AppUserId { get; set; }
        public int InjuryId { get; set; }
        public AppUser AppUser { get; set; }
        public Injury Injury { get; set; }
        public bool IsTooSevere { get; set; } = false;
        public string? DoctorId { get; set; }
        public AppUser? Doctor { get; set; }
    }
}