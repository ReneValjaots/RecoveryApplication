using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecoveryREST.Dtos.UserInjury;
using RecoveryREST.Extensions;
using RecoveryREST.Interfaces;
using RecoveryREST.Models.Classes;

namespace RecoveryREST.Controllers {
    [ApiController]
    [Route("api/userinjury")]
    public class UserInjuryController(IUserInjuryRepo repo, UserManager<AppUser> userManager) : ControllerBase {
        private readonly IUserInjuryRepo _repo = repo;
        private readonly UserManager<AppUser> _userManager = userManager;

        /// <summary>
        /// Returns the injuries assigned to the currently logged-in user.
        /// </summary>
        /// <remarks>
        /// This endpoint allows the authenticated user to fetch all injuries linked to their profile.
        /// 
        /// Sample request:
        /// 
        ///     GET /api/userinjury/user/injuries
        /// 
        /// - If the request is successful, a <c>200 OK</c> response is returned with a list of injuries.
        /// </remarks>
        /// <returns>A list of injuries associated with the user.</returns>
        /// <response code="401">If the user is not logged in</response>
        /// <response code="200">Returns user injuries</response>
        [HttpGet("user/injuries")][Authorize]
        public async Task<IActionResult> GetUserInjuries() {
            var username = User.GetUsername();
            var appUser = await _userManager.FindByNameAsync(username);
            if (appUser == null) return NotFound("User not found.");
            var userInjuries = await _repo.GetUserInjuries(appUser);
            return Ok(userInjuries);
        }
    	
        /// <summary>
        /// Assigns a specified injury to the currently logged-in user.
        /// </summary>
        /// <remarks>
        /// This endpoint allows the authenticated user to assign an injury to their profile using <paramref name="injuryId"/>. 
        /// 
        /// Sample request:
        /// 
        ///     PUT /api/userinjury/assign/{injuryId}
        ///     
        /// Replace <c>{injuryId}</c> with the actual ID of the injury you want to assign.
        ///
        /// - If the injury with the specified <paramref name="injuryId"/> does not exist, a <c>404 NotFound</c> response is returned.
        /// - If the assignment is successful, a <c>200 OK</c> response is returned with a success message.
        /// </remarks>
        /// <param name="injuryId">The ID of the injury to assign to the user.</param>
        /// <returns>Return a message about successful or failed result</returns>
        /// <response code="401">If user is not logged-in</response>
        /// <response code="404">If the injury with the given <paramref name="injuryId"/> was not found or not assigned to the user.</response>
        /// <response code="200">If assignment was successful</response>
        [HttpPut("assign")][Authorize]
        public async Task<IActionResult> AssignInjuryToUser([FromBody] InjurySeverityDto severityDto) {
            if (!ModelState.IsValid) return BadRequest(ModelState);
    
            var username = User.GetUsername();
            var appUser = await _userManager.FindByNameAsync(username);
            if (appUser == null) return NotFound("User not found.");

            var injury = await _repo.AssignInjuryToUser(severityDto.InjuryId, appUser, severityDto);
            if (injury == null) return NotFound($"Injury with ID {severityDto.InjuryId} not found or assignment failed");
            
            return Ok(injury);
        }

        /// <summary>
        /// Unlinks a specified injury from the currently logged-in user.
        /// </summary>
        /// <remarks>
        /// This endpoint allows the authenticated user to remove an injury from their profile using <paramref name="injuryId"/>.
        /// 
        /// Sample request:
        /// 
        ///     PATCH /api/userinjury/unlink/{injuryId}
        ///     
        /// Replace <c>{injuryId}</c> with the actual ID of the injury you want to unlink.
        /// 
        /// - If the injury with the specified <paramref name="injuryId"/> does not exist or is not assigned to the user, a <c>404 Not Found</c> response is returned.
        /// - If the unlinking is successful, a <c>200 OK</c> response is returned with a success message.
        /// </remarks>
        /// <param name="injuryId">The ID of the injury to unlink from the user.</param>
        /// <returns>A message indicating the success or failure of the unlinking operation.</returns>
        /// <response code="401">If the user is not logged in.</response>
        /// <response code="404">If the injury with the given <paramref name="injuryId"/> was not found or not assigned to the user.</response>
        /// <response code="200">If the unlinking was successful.</response>
        [HttpPatch("unlink/{injuryId}")][Authorize] 
        public async Task<IActionResult> RemoveInjuryFromUser(int injuryId) {
            var username = User.GetUsername();
            var appUser = await _userManager.FindByNameAsync(username);

            var result = await _repo.RemoveInjuryFromUser(injuryId, appUser);
            if (!result) return NotFound("Injury not found or not assigned to this user");

            return Ok($"Injury with ID {injuryId} was unlinked successfully.");
        }
    }
}