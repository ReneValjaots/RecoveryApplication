using RecoveryREST.Dtos.RecoveryExercise;
using RecoveryREST.Models.Classes;

namespace RecoveryREST.Interfaces {
    public interface IRecoveryExerciseRepo {
        Task<(RecoveryExerciseInfoDto? exerciseDto, string? errorMessage)> CreateAsync(CreateRecoveryExerciseDto createDto);
        Task<List<RecoveryExerciseInfoDto>> GetAllAsync();
        Task<RecoveryExerciseInfoDto?> GetByIdAsync(int id);
        Task<bool> RecoveryExerciseExistsInDb(int id);
        Task<bool> InjuryExistsInDb(int id);
        Task<(RecoveryExerciseInfoDto? exeriseDto, string? errorMessage)> UpdateAsync(int id, UpdateRecoveryExerciseDto updateDto);
        Task<RecoveryExercise?> DeleteAsync(int id);
        Task LinkRecoveryExerciseToInjury(int exeriseId, int injuryId);
    }
}