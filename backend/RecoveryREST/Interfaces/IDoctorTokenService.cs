using RecoveryREST.Models.Classes;

namespace RecoveryREST.Interfaces {
    public interface IDoctorTokenService {
        Task<string> CreateToken(AppUser user);
        bool IsValidDoctorSecretKey(string secretKey);
    }
}