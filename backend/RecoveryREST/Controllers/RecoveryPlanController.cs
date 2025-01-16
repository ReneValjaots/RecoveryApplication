using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecoveryREST.Dtos.RecoveryPlan;
using RecoveryREST.Extensions;
using RecoveryREST.Interfaces;
using RecoveryREST.Models.Classes;

namespace RecoveryREST.Controllers {
    [ApiController]
    [Route("api/recoveryplan")]
    public class RecoveryPlanController(IRecoveryPlanRepo recoveryPlanRepo, UserManager<AppUser> userManager) : ControllerBase {
        private readonly IRecoveryPlanRepo _recoveryPlanRepo = recoveryPlanRepo;
        private readonly UserManager<AppUser> _userManager = userManager;

        /// <summary>
        /// Retrieves the recovery plan associated with the currently logged-in user.
        /// </summary>
        /// <remarks>
        /// This endpoint allows the authenticated user to fetch their own recovery plan.
        /// 
        /// Sample request:
        /// 
        ///     GET /api/RecoveryPlan
        /// 
        /// - If the user has a recovery plan, a <c>200 OK</c> response is returned with the plan details.
        /// </remarks>
        /// <returns>The user's recovery plan, if available.</returns>
        /// <response code="401">If the user is not logged in</response>
        /// <response code="200">Returns the recovery plan</response>
        [HttpGet][Authorize] public async Task<IActionResult> GetUserRecoveryPlan() {
            var username = User.GetUsername();
            var appUser = await _userManager.FindByNameAsync(username);

            if (appUser == null) return Unauthorized("User not found.");

            var recoveryPlans = await _recoveryPlanRepo.GetUserRecoveryPlan(appUser);

            return Ok(recoveryPlans);
        }

        /// <summary>
        /// Retrieves a specific recovery plan by its ID.
        /// </summary>
        /// <remarks>
        /// This endpoint allows an authenticated user to retrieve a specific recovery plan by ID if they have access to it.
        /// 
        /// Sample request:
        /// 
        ///     GET /api/Recoveryplan/{id}
        ///     
        /// Replace <c>{id}</c> with the actual ID of the recovery plan you want to retrieve.
        /// 
        /// - If the recovery plan does not exist, a <c>404 NotFound</c> response is returned.
        /// - If the user does not have access to the recovery plan, a <c>403 Forbidden</c> response is returned.
        /// - If successful, a <c>200 OK</c> response is returned with the plan details.
        /// </remarks>
        /// <param name="id">The ID of the recovery plan.</param>
        /// <returns>The specified recovery plan if found and accessible.</returns>
        /// <response code="401">If the user is not logged in</response>
        /// <response code="403">If the user does not have access to the specified recovery plan</response>
        /// <response code="404">If the recovery plan with the given <paramref name="id"/> was not found</response>
        /// <response code="200">Returns the recovery plan</response>
        [HttpGet("{id}")] public async Task<IActionResult> GetRecoveryPlanById(int id) {
            var username = User.GetUsername();
            var appUser = await _userManager.FindByNameAsync(username);
            if (appUser == null) return Unauthorized("User not found.");

            var recoveryPlan = await _recoveryPlanRepo.GetRecoveryPlanById(id, appUser.Id);

            if (recoveryPlan == null) return NotFound("Recovery plan not found.");
            
            return Ok(recoveryPlan);
        }

        /// <summary>
        /// Creates a new recovery plan for the currently logged-in user.
        /// </summary>
        /// <remarks>
        /// This endpoint allows an authenticated user to create a recovery plan with a specified name.
        /// 
        /// Sample request:
        /// 
        ///     POST /api/RecoveryPlan
        /// 
        /// Request body:
        /// 
        ///     {
        ///         "name": "New Recovery Plan"
        ///     }
        /// 
        /// - If the user already has an existing recovery plan, a <c>400 BadRequest</c> response is returned.
        /// - If the recovery plan is successfully created, a <c>201 Created</c> response is returned with the created plan's details.
        /// </remarks>
        /// <param name="name">The name of the new recovery plan.</param>
        /// <returns>The newly created recovery plan.</returns>
        /// <response code="401">If the user is not logged in</response>
        /// <response code="400">If the user already has an existing recovery plan or name is null</response>
        /// <response code="201">If the recovery plan is successfully created</response>
        [HttpPost][Authorize] public async Task<IActionResult> CreateRecoveryPlan([FromBody] string name) {
            var username = User.GetUsername();
            var appUser = await _userManager.FindByNameAsync(username);
            if (appUser == null) return Unauthorized("user not found");

            var recoveryPlan = await _recoveryPlanRepo.CreateRecoveryPlanAsync(appUser.Id, name);

            if (recoveryPlan == null)
                return BadRequest("Plan name cannot be empty, contain more than 40 characters or user already has a recovery plan.");

            return CreatedAtAction(nameof(GetRecoveryPlanById), new { id = recoveryPlan.Id }, recoveryPlan);
        }

        /// <summary>
        /// Assigns a recovery exercise to the currently logged-in user's recovery plan.
        /// </summary>
        /// <remarks>
        /// This endpoint allows an authenticated user to add a specific recovery exercise to their recovery plan.
        /// 
        /// Sample request:
        /// 
        ///     PUT /api/RecoveryPlan/assign/{recoveryExerciseId}
        ///     
        /// Replace <c>{recoveryExerciseId}</c> with the actual ID of the recovery exercise to assign.
        /// 
        /// - If the recovery exercise does not exist, a <c>404 NotFound</c> response is returned.
        /// - If the assignment is successful, a <c>200 OK</c> response is returned with a success message.
        /// </remarks>
        /// <param name="recoveryExerciseId">The ID of the recovery exercise to assign.</param>
        /// <returns>A message indicating the result of the assignment.</returns>
        /// <response code="401">If the user is not logged in</response>
        /// <response code="404">If the recovery exercise was not found or could not be assigned</response>
        /// <response code="200">If the recovery exercise was successfully assigned</response>
        [HttpPut("assign/{recoveryExerciseId}/{planId}")][Authorize]
        public async Task<IActionResult> AssignRecoveryToUser(int recoveryExerciseId, int planId, [FromBody] AssignRecoveryExerciseDto dto) {
            var username = User.GetUsername();
            var appUser = await _userManager.FindByNameAsync(username);
            if (appUser == null) return Unauthorized("User not found.");

            if (dto.DayNumber < 1) return BadRequest("Day number must be 1 or greater.");

            var result = await _recoveryPlanRepo.AssignRecoveryExerciseToUser(recoveryExerciseId, planId, appUser, dto.DayNumber, dto.Sets, dto.Reps, dto.Duration);

            if (!result) 
                return NotFound($"Recovery exercise with ID {recoveryExerciseId} or plan ID {planId} not found or assignment failed");
            
            var recoveryPlanDto = await _recoveryPlanRepo.GetRecoveryPlanById(planId, appUser.Id);
            if (recoveryPlanDto == null) return NotFound($"Recovery Plan with ID {planId} not found");

            return Ok(recoveryPlanDto);
        }

        /// <summary>
        /// Removes a recovery exercise from the currently logged-in user's recovery plan.
        /// </summary>
        /// <remarks>
        /// This endpoint allows an authenticated user to unlink a specific recovery exercise from their recovery plan.
        /// 
        /// Sample request:
        /// 
        ///     PATCH api/RecoveryPlan/unlink/{recoveryExerciseId}
        ///     
        /// Replace <c>{recoveryExerciseId}</c> with the actual ID of the recovery exercise to unlink.
        /// 
        /// - If the recovery exercise is not found or not assigned to the user, a <c>404 NotFound</c> response is returned.
        /// - If the unlinking is successful, a <c>200 OK</c> response is returned with a success message.
        /// </remarks>
        /// <param name="recoveryExerciseId">The ID of the recovery exercise to remove.</param>
        /// <returns>A message indicating the result of the operation.</returns>
        /// <response code="401">If the user is not logged in</response>
        /// <response code="404">If the recovery exercise was not found or not assigned to the user</response>
        /// <response code="200">If the recovery exercise was successfully unlinked</response>
        [HttpPatch("unlink/{recoveryExerciseId}/{planId}")][Authorize] 
        public async Task<IActionResult> RemoveRecoveryExerciseFromUser(int recoveryExerciseId, int planId, [FromBody] RemoveRecoveryDto dto) {
            var username = User.GetUsername();
            var appUser = await _userManager.FindByNameAsync(username);
            if (appUser == null) return Unauthorized("User not found.");

            var result = await _recoveryPlanRepo.RemoveRecoveryExerciseFromUser(recoveryExerciseId, planId, appUser, dto.DayNumber);
            if (!result) 
                return NotFound("Recovery Exercise not found or not assigned to this user");

            var recoveryPlanDto = await _recoveryPlanRepo.GetRecoveryPlanById(planId, appUser.Id);
            if (recoveryPlanDto == null) return NotFound($"Recovery Plan with ID {planId} not found"); 

            return Ok(recoveryPlanDto);
        }

        [HttpDelete("{id}")][Authorize]
        public async Task<IActionResult> DeleteRecoveryPlan(int id) {
            var username = User.GetUsername();
            var appUser = await _userManager.FindByNameAsync(username);

            var result = await _recoveryPlanRepo.DeleteRecoveryPlanAsync(id, appUser);

            if (!result)
                return NotFound("Recovery plan not found or you do not have permission to delete this plan.");

            return NoContent(); 
        }
    }
}