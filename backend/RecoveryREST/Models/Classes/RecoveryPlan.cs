namespace RecoveryREST.Models.Classes {
    public class RecoveryPlan {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AppUserId { get; set; } = string.Empty;
        public AppUser AppUser { get; set; }
        public List<WorkoutDay> WorkoutDays { get; set; } = new List<WorkoutDay>();
        public bool IsCreatedByDoctor { get; set; } = false;
        public string? DoctorId { get; set; }
        public AppUser? Doctor { get; set; }
    }
}