using Microsoft.EntityFrameworkCore;
using RecoveryREST.Data;
using RecoveryREST.Dtos.RecoveryPlan;
using RecoveryREST.Interfaces;
using RecoveryREST.Models.Classes;

namespace RecoveryREST.Repos {
    public class RecoveryPlanRepo(ApplicationDbContext context) : IRecoveryPlanRepo {
        private readonly ApplicationDbContext _context = context;

        public async Task<List<RecoveryPlanInfoDto>> GetUserRecoveryPlan(AppUser user) {
            return await _context.RecoveryPlans
                .Where(rp => rp.AppUserId == user.Id)
                .Include(rp => rp.WorkoutDays)
                .ThenInclude(wd => wd.RecoveryPlanExercises)
                .ThenInclude(rpe => rpe.RecoveryExercise)
                .Select(rp => new RecoveryPlanInfoDto {
                    Id = rp.Id,
                    Name = rp.Name,
                    IsCreatedByDoctor = rp.IsCreatedByDoctor,
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

        public async Task<RecoveryPlanInfoDto?> GetRecoveryPlanById(int id, string userId) {
            var recoveryPlan = await _context.RecoveryPlans
                .Include(rp => rp.WorkoutDays)
                .ThenInclude(wd => wd.RecoveryPlanExercises)
                .ThenInclude(rpe => rpe.RecoveryExercise)
                .FirstOrDefaultAsync(rp => rp.Id == id && rp.AppUserId == userId);

            if (recoveryPlan == null) return null;

            return new RecoveryPlanInfoDto {
                Id = recoveryPlan.Id,
                Name = recoveryPlan.Name,
                IsCreatedByDoctor = recoveryPlan.IsCreatedByDoctor,
                WorkoutDays = recoveryPlan.WorkoutDays
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
            };
        }

        public async Task<RecoveryPlanDto?> CreateRecoveryPlanAsync(string userId, string name) {
            if (string.IsNullOrWhiteSpace(name))  
                return null;  

            if (name.Length > 40)  
                return null;  

            var recoveryPlan = new RecoveryPlan {
                Name = name,
                AppUserId = userId,
                WorkoutDays = new List<WorkoutDay>()
            };

            await _context.RecoveryPlans.AddAsync(recoveryPlan);
            await _context.SaveChangesAsync();

            return new RecoveryPlanDto {
                Id = recoveryPlan.Id,
                Name = recoveryPlan.Name,
                WorkoutDays = new List<WorkoutDayDto>()
            };
        }

        public async Task<bool> AssignRecoveryExerciseToUser (int recoveryExerciseId, int planId, AppUser user, int dayNumber, int? sets, int? reps, TimeSpan? duration) {
            if (dayNumber < 1) return false;
            if (sets < 0 || reps < 0) return false;
            var recoveryPlan = await _context.RecoveryPlans
                .Include(rp => rp.WorkoutDays)
                .ThenInclude(wd => wd.RecoveryPlanExercises)
                .FirstOrDefaultAsync(rp => rp.Id == planId && rp.AppUserId == user.Id);

            if (recoveryPlan is null) return false;

            var workoutDay = recoveryPlan.WorkoutDays.FirstOrDefault(wd => wd.DayNumber == dayNumber);
            if (workoutDay == null) {
                workoutDay = new WorkoutDay { 
                    DayNumber = dayNumber, 
                    RecoveryPlanId = planId 
                };
                recoveryPlan.WorkoutDays.Add(workoutDay);
            }

            var existingExercise = workoutDay.RecoveryPlanExercises.FirstOrDefault(rpe => rpe.RecoveryExerciseId == recoveryExerciseId);

            if (existingExercise != null) {
                existingExercise.Sets = sets;
                existingExercise.Reps = reps;
                existingExercise.Duration = duration;
            } else {
                workoutDay.RecoveryPlanExercises.Add(new RecoveryPlanExercise {
                    RecoveryExerciseId = recoveryExerciseId,
                    Sets = sets,
                    Reps = reps,
                    Duration = duration
                });
            }
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveRecoveryExerciseFromUser(int recoveryExerciseId, int planId, AppUser user, int dayNumber) {
            var recoveryPlan = await _context.RecoveryPlans
                .Include(rp => rp.WorkoutDays)
                .ThenInclude(wd => wd.RecoveryPlanExercises)
                .FirstOrDefaultAsync(rp => rp.Id == planId && rp.AppUserId == user.Id);

            if (recoveryPlan is null) return false;

            var workoutDay = recoveryPlan.WorkoutDays.FirstOrDefault(wd => wd.DayNumber == dayNumber);
            if (workoutDay == null) return false;

            var recoveryPlanExercise = workoutDay.RecoveryPlanExercises
                .FirstOrDefault(rpe => rpe.RecoveryExerciseId == recoveryExerciseId);
            if (recoveryPlanExercise == null) return false;

            workoutDay.RecoveryPlanExercises.Remove(recoveryPlanExercise);

            if (!workoutDay.RecoveryPlanExercises.Any()) recoveryPlan.WorkoutDays.Remove(workoutDay);
    
            await _context.SaveChangesAsync();
            return true; 
        }

        public async Task<bool> DeleteRecoveryPlanAsync(int planId, AppUser user) {
            var recoveryPlan = await _context.RecoveryPlans
                .FirstOrDefaultAsync(rp => rp.Id == planId && rp.AppUserId == user.Id);

            if (recoveryPlan == null) return false;

            _context.RecoveryPlans.Remove(recoveryPlan);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}