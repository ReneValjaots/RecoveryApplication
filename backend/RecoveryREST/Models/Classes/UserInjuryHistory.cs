namespace RecoveryREST.Models.Classes {
    public class UserInjuryHistory {
        public int Id { get; set; } 
        public string AppUserId { get; set; }
        public int InjuryId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; } 
        
        public AppUser AppUser { get; set; }
        public Injury Injury { get; set; }
    }
}