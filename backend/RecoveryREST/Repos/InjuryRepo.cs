using Microsoft.EntityFrameworkCore;
using RecoveryREST.Data;
using RecoveryREST.Dtos.Injury;
using RecoveryREST.Dtos.RecoveryExercise;
using RecoveryREST.Interfaces;
using RecoveryREST.Models.Classes;

namespace RecoveryREST.Repos {
    public class InjuryRepo(ApplicationDbContext context) : IInjuryRepo {
        private readonly ApplicationDbContext _context = context;

        public async Task<(InjuryInfoDto? injuryDto, string? errorMessage)> CreateAsync(CreateInjuryDto createDto) {
            var exerciseIdsToValidate = (createDto.RecoveryExerciseIds ?? new List<int>())
                .Where(id => id != 0)
                .ToList();

            if (exerciseIdsToValidate.Count != 0) {
                var validExerciseIds = await _context.RecoveryExercises
                    .Where(re => exerciseIdsToValidate.Contains(re.Id))
                    .Select(re => re.Id)
                    .ToListAsync();

                var invalidExerciseIds = exerciseIdsToValidate.Except(validExerciseIds).ToList();

                if (invalidExerciseIds.Count != 0)
                    return (null, $"Some recovery exercises could not be linked due to invalid IDs: {string.Join(", ", invalidExerciseIds)}");
            }

            var injury = new Injury {
                Name = createDto.Name,
                Description = createDto.Description,
                BodyPart = createDto.BodyPart,
            };

            _context.Injuries.Add(injury);
            await _context.SaveChangesAsync();

            foreach (var exerciseId in exerciseIdsToValidate) {
                await LinkInjuryToRecoveryExercise(injury.Id, exerciseId);
            }

            var savedInjury = await GetInfoDtoAsync(injury.Id);
            return (savedInjury, null);
        }

        public async Task<List<InjuryInfoDto>> GetAllAsync() {
            return await _context.Injuries
                .Include(i => i.InjuryRecoveryExercises)
                .ThenInclude(ire => ire.RecoveryExercise)
                .Select(i => MapToInfoDto(i))
                .ToListAsync();
        }

        public async Task<InjuryInfoDto?> GetByIdAsync(int id) => await GetInfoDtoAsync(id);
        public async Task<bool> InjuryExistsInDb(int id) => await _context.Injuries.AnyAsync(x => x.Id == id);
        public async Task<bool> RecoveryExerciseExistsInDb(int id) => await _context.RecoveryExercises.AnyAsync(x => x.Id == id);

        public async Task<(InjuryInfoDto? injuryDto, string? errorMessage)> UpdateAsync(int id, UpdateInjuryDto updateDto) {
            var existingInjury = await _context.Injuries
                .Include(i => i.InjuryRecoveryExercises)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (existingInjury == null) return (null, "Injury not found.");

            existingInjury.Name = updateDto.Name;
            existingInjury.Description = updateDto.Description;
            existingInjury.BodyPart = updateDto.BodyPart;

            existingInjury.InjuryRecoveryExercises.Clear();

            if (updateDto.RecoveryExerciseIds != null && updateDto.RecoveryExerciseIds.Count != 0) {
                var validExerciseIds = new List<int>();
                var invalidExerciseIds = new List<int>();

                foreach (var exerciseId in updateDto.RecoveryExerciseIds) {
                    if (exerciseId == 0) continue;
                    if (await RecoveryExerciseExistsInDb(exerciseId)) {
                        validExerciseIds.Add(exerciseId);
                    } else {
                        invalidExerciseIds.Add(exerciseId);
                    }
                }

                if (invalidExerciseIds.Count != 0) 
                    return (null, $"Invalid Recovery Exercise IDs: {string.Join(", ", invalidExerciseIds)}");

                foreach (var exerciseId in validExerciseIds) {
                    await LinkInjuryToRecoveryExercise(existingInjury.Id, exerciseId);
                } 
            }

            _context.Update(existingInjury);
            await _context.SaveChangesAsync();

            var updatedInjuryDto = await GetInfoDtoAsync(existingInjury.Id);
            return (updatedInjuryDto, null);
        }

        public async Task<Injury?> DeleteAsync(int id) {
            var injury = await _context.Injuries.FirstOrDefaultAsync(x => x.Id == id);
            if (injury is null) return null;

            _context.Injuries.Remove(injury);
            await _context.SaveChangesAsync();

            return injury;
        }

        public async Task LinkInjuryToRecoveryExercise(int injuryId, int exerciseId) {
            var injury = await GetInjuryEntityById(injuryId);
            var recoveryExercise = await _context.RecoveryExercises.FindAsync(exerciseId);

            if (injury != null && recoveryExercise != null) {
                injury.InjuryRecoveryExercises.Add(new InjuryRecoveryExercise {
                    RecoveryExerciseId = exerciseId,
                    InjuryId = injuryId
                });

                await _context.SaveChangesAsync();
            }
        }

        private async Task<InjuryInfoDto?> GetInfoDtoAsync(int id) {
            return await _context.Injuries
                .Include(i => i.InjuryRecoveryExercises)
                .ThenInclude(ire => ire.RecoveryExercise)
                .Where(i => i.Id == id)
                .Select(i => MapToInfoDto(i))
                .FirstOrDefaultAsync();
        }

        private static InjuryInfoDto MapToInfoDto (Injury injury) {
            return new InjuryInfoDto {
                Id = injury.Id,
                Name = injury.Name,
                Description = injury.Description,
                BodyPart = injury.BodyPart,
                RecoveryExercises = injury.InjuryRecoveryExercises
                    .Select(ire => new RecoveryExerciseDto {
                        Id = ire.RecoveryExercise.Id,
                        Name = ire.RecoveryExercise.Name,
                        Description = ire.RecoveryExercise.Description,
                    }).ToList()
            };
        }

        private async Task<Injury?> GetInjuryEntityById(int id) {
            return await _context.Injuries
                .Include(i => i.InjuryRecoveryExercises)
                .ThenInclude(ire => ire.RecoveryExercise)
                .FirstOrDefaultAsync(i => i.Id == id);
        }
    }
}