using Microsoft.EntityFrameworkCore;
using RecoveryREST.Data;
using RecoveryREST.Dtos.RecoveryExercise;
using RecoveryREST.Dtos.UserInjury;
using RecoveryREST.Interfaces;
using RecoveryREST.Models.Classes;

namespace RecoveryREST.Repos {
    public class UserInjuryRepo(ApplicationDbContext context) : IUserInjuryRepo {
        private readonly ApplicationDbContext _context = context;

        public async Task<List<UserInjuryDto>> GetUserInjuries(AppUser appUser) {
            return await _context.UserInjuries
                .Where(x => x.AppUserId == appUser.Id)
                .Include(ui => ui.Injury)
                .Select(ui => new UserInjuryDto {
                    Id = ui.Injury.Id,
                    Name = ui.Injury.Name,
                    Description = ui.Injury.Description,
                    AppUserId = ui.AppUserId,
                    IsTooSevere = ui.IsTooSevere,
                    RecoveryExercises = ui.Injury.InjuryRecoveryExercises
                        .Select(ire => new RecoveryExerciseDto {
                            Id = ire.RecoveryExercise.Id,
                            Name = ire.RecoveryExercise.Name,
                            Description = ire.RecoveryExercise.Description
                        }).ToList()
                }).ToListAsync();
        }
        
        public async Task<AssignedInjuryDto?> AssignInjuryToUser(int injuryId, AppUser user, InjurySeverityDto severityDto) {
            var injury = await _context.Injuries.FirstOrDefaultAsync(i => i.Id == injuryId);
            if (injury is null) return null;

            var activeInjury = await _context.UserInjuries
                .FirstOrDefaultAsync(ui => ui.AppUserId == user.Id && ui.InjuryId == injuryId);

            bool isNewInjury = false;
            if (activeInjury == null) {
                var userInjury = new UserInjury { 
                    AppUserId = user.Id,
                    InjuryId = injuryId,
                    IsTooSevere = severityDto?.IsTooSevere ?? false,
                };
                await _context.UserInjuries.AddAsync(userInjury);
                isNewInjury = true;
            } else {
                activeInjury.IsTooSevere = severityDto?.IsTooSevere ?? false;
                _context.UserInjuries.Update(activeInjury);
                isNewInjury = false;
            }

            if (isNewInjury) {
                var injuryHistory = new UserInjuryHistory {
                    AppUserId = user.Id,
                    InjuryId = injuryId,
                    StartDate = DateTime.UtcNow
                };
                await _context.UserInjuryHistories.AddAsync(injuryHistory);
            }
          
            var injuryDto = new AssignedInjuryDto {
                InjuryId = injury.Id,
                Name = injury.Name,
                Description = injury.Description,
                IsTooSevere = severityDto?.IsTooSevere ?? false
            };

            await _context.SaveChangesAsync();
            return injuryDto;
        }

        public async Task<bool> RemoveInjuryFromUser(int injuryId, AppUser user) {
            var userInjury = await _context.UserInjuries.FirstOrDefaultAsync(ui => ui.InjuryId == injuryId && ui.AppUserId == user.Id);

            if (userInjury == null) return false;

            _context.UserInjuries.Remove(userInjury);

             var injuryHistory = await _context.UserInjuryHistories
                .Where(uh => uh.AppUserId == user.Id && uh.InjuryId == injuryId && uh.EndDate == null)
                .OrderByDescending(uh => uh.StartDate) 
                .FirstOrDefaultAsync();

            if (injuryHistory != null) {
                injuryHistory.EndDate = DateTime.UtcNow;
                _context.UserInjuryHistories.Update(injuryHistory);
            }
            await _context.SaveChangesAsync();
            return true; 
        }
    }
}