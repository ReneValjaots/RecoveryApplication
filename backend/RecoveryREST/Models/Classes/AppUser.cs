using Microsoft.AspNetCore.Identity;

namespace RecoveryREST.Models.Classes {
    public class AppUser : IdentityUser {
        public List<UserInjury> UserInjuries { get; set; } = new List<UserInjury>();
        public List<RecoveryPlan> RecoveryPlans { get; set; } = new List<RecoveryPlan>();
    }
}