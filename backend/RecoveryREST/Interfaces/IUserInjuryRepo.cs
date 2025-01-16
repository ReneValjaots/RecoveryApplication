using RecoveryREST.Dtos.UserInjury;
using RecoveryREST.Models.Classes;

namespace RecoveryREST.Interfaces {
    public interface IUserInjuryRepo {
        Task<List<UserInjuryDto>> GetUserInjuries(AppUser appUser);
        Task<AssignedInjuryDto?> AssignInjuryToUser(int injuryId, AppUser user, InjurySeverityDto severityDto);
        Task<bool> RemoveInjuryFromUser(int injuryId, AppUser user);
    }
}