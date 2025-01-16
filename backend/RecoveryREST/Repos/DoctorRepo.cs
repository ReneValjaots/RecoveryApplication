using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecoveryREST.Data;
using RecoveryREST.Dtos.Doctor;
using RecoveryREST.Dtos.RecoveryPlan;
using RecoveryREST.Interfaces;
using RecoveryREST.Models.Classes;

namespace RecoveryREST.Repos {
    public class DoctorRepo(ApplicationDbContext context, UserManager<AppUser> userManager) : IDoctorRepo {
        private readonly ApplicationDbContext _context = context;
        private readonly UserManager<AppUser> _userManager = userManager;

        public async Task<List<DoctorPatientDto>> GetAllPatientsAsync() {
            return await _context.UserInjuries
                .Where(ui => ui.IsTooSevere)
                .Select(ui => new DoctorPatientDto {
                    AppUserId = ui.AppUserId,
                    InjuryId = ui.InjuryId,
                    Username = ui.AppUser.UserName,
                    IsTooSevere = ui.IsTooSevere
                })
                .ToListAsync();
        }
        
        public async Task<List<DoctorPatientDto>> GetAllDoctorPatientsAsync(string doctorId) {
            return await _context.UserInjuries
                .Where(ui => ui.DoctorId == doctorId)
                .Select(ui => new DoctorPatientDto {
                    AppUserId = ui.AppUserId,
                    InjuryId = ui.InjuryId,
                    Username = ui.AppUser.UserName,
                    IsTooSevere = ui.IsTooSevere
                })
                .ToListAsync();
        }

        public async Task<List<DoctorPatientDto>> GetAllAvailablePatientsAsync() {
            return await _context.UserInjuries
                .Where(ui => ui.IsTooSevere && ui.DoctorId == null)
                .Select(ui => new DoctorPatientDto {
                     AppUserId = ui.AppUserId,
                    InjuryId = ui.InjuryId,
                    Username = ui.AppUser.UserName,
                    IsTooSevere = ui.IsTooSevere
                })
                .ToListAsync();
        }

        public async Task<UserInjury?> GetUserInjuryAsync(string appUserId, int injuryId) {
            return await _context.UserInjuries
                .FirstOrDefaultAsync(ui => ui.AppUserId == appUserId && ui.InjuryId == injuryId);
        }

        public async Task AssignDoctorToInjury(UserInjury userInjury, string doctorId) {
            userInjury.DoctorId = doctorId;
            _context.UserInjuries.Update(userInjury);
            await _context.SaveChangesAsync();
        }

        public async Task UnassignDoctorFromInjuryAsync(UserInjury userInjury) {
            userInjury.DoctorId = null;
            _context.UserInjuries.Update(userInjury);
            await _context.SaveChangesAsync();
        }

        public async Task<List<RecoveryPlanAppUserInfoDto>> GetAllRecoveryPlansAsync(string doctorId) {
            return await _context.RecoveryPlans
                .Where(rp => rp.DoctorId == doctorId && rp.IsCreatedByDoctor)
                .Include(rp => rp.WorkoutDays)
                .ThenInclude(wd => wd.RecoveryPlanExercises)
                .ThenInclude(rpe => rpe.RecoveryExercise)
                .Select(rp => new RecoveryPlanAppUserInfoDto {
                    Id = rp.Id,
                    Name = rp.Name,
                    AppUserId = rp.AppUserId,
                    WorkoutDays = rp.WorkoutDays
                        .OrderBy(wd => wd.DayNumber)
                        .Select(wd => new WorkoutDayDto {
                            DayNumber = wd.DayNumber,
                            Exercises = wd.RecoveryPlanExercises.Select(rpe => new RecoveryExerciseDetailDto {
                                Id = rpe.RecoveryExercise.Id,
                                Name = rpe.RecoveryExercise.Name,
                                Description = rpe.RecoveryExercise.Description,
                                Sets = rpe.Sets,
                                Reps = rpe.Reps,
                                Duration = rpe.Duration
                            }).ToList()
                        }).ToList()
                }).ToListAsync() ?? [];
        }

        public async Task<(RecoveryPlanAppUserInfoDto?, string?)> CreateRecoveryPlanAsync(CreateRecoveryPlanDto dto, string doctorId) {
            var user = await _userManager.FindByIdAsync(dto.AppUserId);
            if (user == null) return (null, "Invalid user ID.");

            if (string.IsNullOrEmpty(dto.Name.Trim()))
                return (null, "Plan name can not be empty");  

            if (dto.Name.Length > 40)  
                return (null, "Plan name needs to be less then 40 characters");

            if (dto.WorkoutDays == null || !dto.WorkoutDays.Any())
                return (null, "Workout days cannot be empty.");

            var invalidItem = dto.WorkoutDays
                .SelectMany(wd => wd.Exercises)
                .FirstOrDefault(e => (e.Sets.HasValue && e.Sets.Value < 0) || (e.Reps.HasValue && e.Reps.Value < 0));

            if (invalidItem != null)
                return (null, "Sets and reps cannot be less than zero.");

            var allExerciseIds = dto.WorkoutDays?
                .SelectMany(wd => wd.Exercises.Select(e => e.Id))
                .Distinct()
                .ToList() ?? new List<int>();

            if (allExerciseIds.Count > 0) {
                var validExerciseIds = await _context.RecoveryExercises
                    .Where(re => allExerciseIds.Contains(re.Id))
                    .Select(re => re.Id)
                    .ToListAsync();

                var invalidIds = allExerciseIds.Except(validExerciseIds).ToList();
                if (invalidIds.Count > 0)
                    return (null, $"The following exercise IDs are invalid: {string.Join(", ", invalidIds)}");
            }

            var invalidDayNumbers = dto.WorkoutDays?
                .Where(wd => wd.DayNumber < 1)
                .Select(wd => wd.DayNumber)
                .Distinct()
                .ToList();

            if (invalidDayNumbers != null && invalidDayNumbers.Count > 0)
                return (null, $"The following day numbers are invalid: {string.Join(", ", invalidDayNumbers)}");

            var recoveryPlan = new RecoveryPlan {
                Name = dto.Name,
                AppUserId = dto.AppUserId,
                IsCreatedByDoctor = true,
                DoctorId = doctorId,
                WorkoutDays = new List<WorkoutDay>()
            };
            
            foreach (var workoutDayDto in dto.WorkoutDays) {
                var workoutDay = new WorkoutDay {
                    DayNumber = workoutDayDto.DayNumber,
                    RecoveryPlanExercises = new List<RecoveryPlanExercise>()
                };

                foreach (var exerciseDto in workoutDayDto.Exercises) {
                    var recoveryExercise = await _context.RecoveryExercises.FirstOrDefaultAsync(x => x.Id == exerciseDto.Id);
                    if (recoveryExercise == null) continue;

                    workoutDay.RecoveryPlanExercises.Add(new RecoveryPlanExercise{
                        RecoveryExerciseId = exerciseDto.Id,
                        Sets = exerciseDto.Sets,
                        Reps = exerciseDto.Reps,
                        Duration = exerciseDto.Duration,
                    });
                }
                recoveryPlan.WorkoutDays.Add(workoutDay);
            }

            await _context.RecoveryPlans.AddAsync(recoveryPlan);
            await _context.SaveChangesAsync();

            var recoveryPlanDto = await GetRecoveryPlanByIdAsync(recoveryPlan.Id, doctorId);
            if (recoveryPlanDto == null) return (null, "Failed to retrieve the created recovery plan.");

            return (recoveryPlanDto, null);
        }

        public async Task<RecoveryPlanAppUserInfoDto?> GetRecoveryPlanByIdAsync(int id, string doctorId) {
             var recoveryPlan = await _context.RecoveryPlans
                .Include(rp => rp.WorkoutDays)
                .ThenInclude(wd => wd.RecoveryPlanExercises)
                .ThenInclude(rpe => rpe.RecoveryExercise)
                .FirstOrDefaultAsync(rp => rp.Id == id && rp.DoctorId == doctorId);

            if (recoveryPlan == null) return null;

            return new RecoveryPlanAppUserInfoDto {
                Id = recoveryPlan.Id,
                Name = recoveryPlan.Name,
                AppUserId = recoveryPlan.AppUserId,
                WorkoutDays = recoveryPlan.WorkoutDays.Select(wd => new WorkoutDayDto {
                    DayNumber = wd.DayNumber,
                    Exercises = wd.RecoveryPlanExercises.Select(rpe => new RecoveryExerciseDetailDto {
                        Id = rpe.RecoveryExercise.Id,
                        Name = rpe.RecoveryExercise.Name,
                        Description = rpe.RecoveryExercise.Description,
                        Sets = rpe.Sets,
                        Reps = rpe.Reps,
                        Duration = rpe.Duration
                    }).ToList()
                }).ToList()
            };
        }

        public async Task<(RecoveryPlanAppUserInfoDto?, string?)> UpdateRecoveryPlanAsync(int id, DoctorUpdatePlanDto updateDto, string doctorId) {
            var existingPlan = await _context.RecoveryPlans
                .Include(rp => rp.WorkoutDays)
                .ThenInclude(wd => wd.RecoveryPlanExercises)
                .FirstOrDefaultAsync(rp => rp.Id == id && rp.IsCreatedByDoctor && rp.DoctorId == doctorId);

            if (string.IsNullOrEmpty(updateDto.Name.Trim()))
                return (null, "Plan name can not be empty");  

            if (updateDto.Name.Length > 40)  
                return (null, "Plan name needs to be less then 40 characters");

            if (existingPlan == null)
                return (null, "Recovery plan not found.");

            if (updateDto.WorkoutDays == null || !updateDto.WorkoutDays.Any())
                return (null, "Workout days cannot be empty.");

            var invalidItem = updateDto.WorkoutDays
                .SelectMany(wd => wd.Exercises)
                .FirstOrDefault(e => (e.Sets.HasValue && e.Sets.Value < 0) || (e.Reps.HasValue && e.Reps.Value < 0));

            if (invalidItem != null)
                return (null, "Sets and reps cannot be less than zero.");

            var allExerciseIds = updateDto.WorkoutDays
                .SelectMany(wd => wd.Exercises.Select(e => e.Id))
                .Distinct()
                .ToList();

            if (allExerciseIds.Count != 0) {
                var validExerciseIds = await _context.RecoveryExercises
                    .Where(re => allExerciseIds.Contains(re.Id))
                    .Select(re => re.Id)
                    .ToListAsync();

                var invalidIds = allExerciseIds.Except(validExerciseIds).ToList();
                if (invalidIds.Count > 0)
                    return (null, $"The following exercise IDs are invalid: {string.Join(", ", invalidIds)}");
            }

            var invalidDayNumbers = updateDto.WorkoutDays?
                .Where(wd => wd.DayNumber < 1)
                .Select(wd => wd.DayNumber)
                .Distinct()
                .ToList();

            if (invalidDayNumbers != null && invalidDayNumbers.Count > 0)
                return (null, $"The following day numbers are invalid: {string.Join(", ", invalidDayNumbers)}");

            _context.WorkoutDays.RemoveRange(existingPlan.WorkoutDays);
            await _context.SaveChangesAsync(); 

            existingPlan.Name = updateDto.Name;

            existingPlan.WorkoutDays = updateDto.WorkoutDays.Select(updatedWorkoutDay => new WorkoutDay {
                DayNumber = updatedWorkoutDay.DayNumber,
                RecoveryPlanId = existingPlan.Id,
                RecoveryPlanExercises = updatedWorkoutDay.Exercises.Select(ex => new RecoveryPlanExercise {
                    RecoveryExerciseId = ex.Id,
                    Sets = ex.Sets,
                    Reps = ex.Reps,
                    Duration = ex.Duration
                }).ToList()
            }).ToList();

            await _context.SaveChangesAsync();

            var updatedPlanDto = await GetRecoveryPlanByIdAsync(existingPlan.Id, doctorId);
            if (updatedPlanDto == null)
                return (null, "Failed to retrieve the updated recovery plan.");

            return (updatedPlanDto, null);
        }

        public async Task DeleteRecoveryPlanAsync(int id, string doctorId) {
            var recoveryPlan = await _context.RecoveryPlans
                .FirstOrDefaultAsync(rp => rp.Id == id && rp.DoctorId == doctorId && rp.IsCreatedByDoctor) 
                    ?? throw new InvalidOperationException("Recovery plan not found or not created by this doctor.");
                    
            _context.RecoveryPlans.Remove(recoveryPlan);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> RecoveryExerciseExistsInDb(int id) => await _context.RecoveryExercises.AnyAsync(x => x.Id == id);

        public async Task<bool> IsDoctorLinkedToUserAsync(string doctorId, string userId) {
            return await _context.UserInjuries
                .AnyAsync(ui => ui.DoctorId == doctorId && ui.AppUserId == userId);
        }
    }
}