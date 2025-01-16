using Microsoft.EntityFrameworkCore;
using RecoveryREST.Data;
using RecoveryREST.Dtos.Injury;
using RecoveryREST.Dtos.RecoveryExercise;
using RecoveryREST.Interfaces;
using RecoveryREST.Models.Classes;

namespace RecoveryREST.Repos {
    public class RecoveryExerciseRepo(ApplicationDbContext context) : IRecoveryExerciseRepo {
        private readonly ApplicationDbContext _context = context;

        public async Task<(RecoveryExerciseInfoDto? exerciseDto, string? errorMessage)> CreateAsync(CreateRecoveryExerciseDto createDto) {
            var injuryIdsToValidate = (createDto.InjuryIds ?? new List<int>())
                .Where(id => id != 0)
                .ToList();

            if (injuryIdsToValidate.Count != 0) {
                var validInjuryIds = await _context.Injuries
                    .Where(i => injuryIdsToValidate.Contains(i.Id))
                    .Select(i => i.Id)
                    .ToListAsync();

                var invalidInjuryIds = injuryIdsToValidate.Except(validInjuryIds).ToList();

                if (invalidInjuryIds.Count != 0)
                    return (null, $"Some injuries could not be linked due to invalid IDs: {string.Join(", ", invalidInjuryIds)}");
            }

            var recoveryExercise = new RecoveryExercise {
                Name = createDto.Name,
                Description = createDto.Description,
            };

            _context.RecoveryExercises.Add(recoveryExercise);
            await _context.SaveChangesAsync();

            foreach (var injuryId in injuryIdsToValidate) {
                await LinkRecoveryExerciseToInjury(recoveryExercise.Id, injuryId);
            }

            var savedRecoveryExercise = await GetInfoDtoAsync(recoveryExercise.Id);
            return (savedRecoveryExercise, null);
        }

        public async Task<List<RecoveryExerciseInfoDto>> GetAllAsync() {
            return await _context.RecoveryExercises
                .Include(re => re.InjuryRecoveryExercises)
                .ThenInclude(ire => ire.Injury)
                .Select(re => MapToInfoDto(re))
                .ToListAsync();
        }

        public async Task<RecoveryExerciseInfoDto?> GetByIdAsync(int id) => await GetInfoDtoAsync(id);
        public async Task<bool> RecoveryExerciseExistsInDb(int id) => await _context.RecoveryExercises.AnyAsync(x => x.Id == id);
        public async Task<bool> InjuryExistsInDb(int id) => await _context.Injuries.AnyAsync(x => x.Id == id);

        public async Task<(RecoveryExerciseInfoDto? exeriseDto, string? errorMessage)> UpdateAsync(int id, UpdateRecoveryExerciseDto updateDto) {
            var existingExercise = await _context.RecoveryExercises
                .Include(re => re.InjuryRecoveryExercises) 
                .FirstOrDefaultAsync(re => re.Id == id);

            if (existingExercise == null) return (null, "Recovery Exercise not found.");

            existingExercise.Name = updateDto.Name;
            existingExercise.Description = updateDto.Description;

            existingExercise.InjuryRecoveryExercises.Clear();

            if (updateDto.InjuryIds != null && updateDto.InjuryIds.Count != 0) {
                var validInjuryIds = new List<int>();
                var invalidInjuryIds = new List<int>();

                foreach (var injuryId in updateDto.InjuryIds) {
                    if (injuryId == 0) continue;
                    if (await InjuryExistsInDb(injuryId)) {
                        validInjuryIds.Add(injuryId);
                    } else {
                        invalidInjuryIds.Add(injuryId);
                    }
                }

                if (invalidInjuryIds.Count != 0) 
                    return (null, $"Invalid Injury IDs: {string.Join(", ", invalidInjuryIds)}");
                
                foreach (var injuryId in validInjuryIds) {
                    await LinkRecoveryExerciseToInjury(existingExercise.Id, injuryId);
                }
            }

            _context.Update(existingExercise);
            await _context.SaveChangesAsync();

            var updatedExerciseDto = await GetInfoDtoAsync(existingExercise.Id);
            return (updatedExerciseDto, null);
        }

        public async Task<RecoveryExercise?> DeleteAsync(int id) {
            var recoveryExercise = await _context.RecoveryExercises.FirstOrDefaultAsync(x => x.Id == id);
            if (recoveryExercise is null) return null;

            _context.RecoveryExercises.Remove(recoveryExercise);
            await _context.SaveChangesAsync();

            return recoveryExercise;
        }

        public async Task LinkRecoveryExerciseToInjury(int exeriseId, int injuryId){
            var recoveryExercise = await GetRecoveryExerciseEntityById(exeriseId);
            var injury = await _context.Injuries.FindAsync(injuryId);

            if (recoveryExercise != null && injury != null) {
                recoveryExercise.InjuryRecoveryExercises.Add(new InjuryRecoveryExercise {
                    RecoveryExerciseId = exeriseId,
                    InjuryId = injuryId
                });

                await _context.SaveChangesAsync();
            }
        }

        private async Task<RecoveryExerciseInfoDto?> GetInfoDtoAsync(int id) {
            return await _context.RecoveryExercises
                .Include(re => re.InjuryRecoveryExercises)
                .ThenInclude(ire => ire.Injury)
                .Where(re => re.Id == id)
                .Select(re => MapToInfoDto(re))
                .FirstOrDefaultAsync();
        }

        private static RecoveryExerciseInfoDto MapToInfoDto (RecoveryExercise re) {
            return new RecoveryExerciseInfoDto {
                Id = re.Id,
                Name = re.Name,
                Description = re.Description,
                Injuries = re.InjuryRecoveryExercises
                    .Select(ire => new InjuryDto {
                        Id = ire.Injury.Id,
                        Name = ire.Injury.Name,
                        Description = ire.Injury.Description
                    }).ToList()
            };
        }

        private async Task<RecoveryExercise?> GetRecoveryExerciseEntityById(int id) {
            return await _context.RecoveryExercises
                .Include(re => re.InjuryRecoveryExercises)
                .ThenInclude(ire => ire.Injury)
                .FirstOrDefaultAsync(i => i.Id == id);
        }
    }
}