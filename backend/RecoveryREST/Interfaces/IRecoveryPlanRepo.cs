using RecoveryREST.Dtos.RecoveryPlan;
using RecoveryREST.Models.Classes;

namespace RecoveryREST.Interfaces {
    public interface IRecoveryPlanRepo {
        Task<List<RecoveryPlanInfoDto>> GetUserRecoveryPlan(AppUser user);
        Task<RecoveryPlanInfoDto?> GetRecoveryPlanById(int id, string userId);
        Task<RecoveryPlanDto?> CreateRecoveryPlanAsync(string userId, string name);
        Task<bool> AssignRecoveryExerciseToUser (int recoveryExerciseId, int planId, AppUser user, int dayNumber, int? sets, int? reps, TimeSpan? duration);
        Task<bool> RemoveRecoveryExerciseFromUser(int recoveryExerciseId, int planId, AppUser user, int dayNumber);
        Task<bool> DeleteRecoveryPlanAsync(int planId, AppUser user);
    }
}