using RecoveryREST.Dtos.Injury;
using RecoveryREST.Models.Classes;

namespace RecoveryREST.Interfaces {
    public interface IInjuryRepo {
        Task<(InjuryInfoDto? injuryDto, string? errorMessage)> CreateAsync(CreateInjuryDto createDto);
        Task<List<InjuryInfoDto>> GetAllAsync();
        Task<InjuryInfoDto?> GetByIdAsync(int id);
        Task<bool> InjuryExistsInDb(int id);
        Task<bool> RecoveryExerciseExistsInDb(int id);
        Task<(InjuryInfoDto? injuryDto, string? errorMessage)> UpdateAsync(int id, UpdateInjuryDto updateDto);
        Task<Injury?> DeleteAsync(int id);
        Task LinkInjuryToRecoveryExercise(int injuryId, int exerciseId);
    }
}