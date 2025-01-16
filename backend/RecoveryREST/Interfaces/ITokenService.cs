using RecoveryREST.Models.Classes;

namespace RecoveryREST.Interfaces {
    public interface ITokenService {
        Task<string> CreateToken(AppUser user);
    }
}