using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecoveryREST.Data;
using RecoveryREST.Extensions;
using RecoveryREST.Models.Classes;

namespace RecoveryREST.Controllers {
    [ApiController]
    [Route("api/statistics")]
    public class StatisticsController(ApplicationDbContext context, UserManager<AppUser> userManager) : ControllerBase {
        private readonly ApplicationDbContext _context = context;
        private readonly UserManager<AppUser> _userManager = userManager;

        [HttpGet("user/injury-history")] [Authorize]
        public async Task<IActionResult> GetUserInjuryHistory() {
            var username = User.GetUsername();
            var appUser = await _userManager.FindByNameAsync(username);

            if (appUser == null) return NotFound("User not found.");

            var injuryHistory = await _context.UserInjuryHistories
                .Where(ui => ui.AppUserId == appUser.Id)
                .Include(ui => ui.Injury)
                .Select(ui => new {
                    InjuryId = ui.Injury.Id,
                    InjuryName = ui.Injury.Name,
                    StartDate = ui.StartDate,
                    EndDate = ui.EndDate
                })
                .ToListAsync();

            return Ok(injuryHistory);
        }
    }
}