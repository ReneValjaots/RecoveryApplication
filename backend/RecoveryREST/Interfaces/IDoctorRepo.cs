using RecoveryREST.Dtos.Doctor;
using RecoveryREST.Dtos.RecoveryPlan;
using RecoveryREST.Dtos.UserInjury;
using RecoveryREST.Models.Classes;

namespace RecoveryREST.Interfaces
{
    public interface IDoctorRepo {
        Task<List<DoctorPatientDto>> GetAllPatientsAsync();
        Task<List<DoctorPatientDto>> GetAllDoctorPatientsAsync(string doctorId);
        Task<UserInjury?> GetUserInjuryAsync(string appUserId, int injuryId);
        Task AssignDoctorToInjury(UserInjury userInjury, string doctorId);
        Task<(RecoveryPlanAppUserInfoDto?, string?)> CreateRecoveryPlanAsync(CreateRecoveryPlanDto dto, string doctorId);
        Task<RecoveryPlanAppUserInfoDto?> GetRecoveryPlanByIdAsync(int id, string doctorId);
        Task<bool> IsDoctorLinkedToUserAsync(string doctorId, string userId);
        Task<(RecoveryPlanAppUserInfoDto?, string?)> UpdateRecoveryPlanAsync(int id, DoctorUpdatePlanDto updateDto, string doctorId);
        Task<List<RecoveryPlanAppUserInfoDto>> GetAllRecoveryPlansAsync(string doctorId);
        Task UnassignDoctorFromInjuryAsync(UserInjury userInjury);
        Task DeleteRecoveryPlanAsync(int id, string doctorId);
        Task<List<DoctorPatientDto>> GetAllAvailablePatientsAsync();
    }
}