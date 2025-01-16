using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecoveryREST.Dtos.RecoveryExercise;
using RecoveryREST.Interfaces;

namespace RecoveryREST.Controllers {
    [ApiController]
    [Route("api/recoveryexercise")]
    public class RecoveryExerciseController(IRecoveryExerciseRepo repo) : ControllerBase {
        private readonly IRecoveryExerciseRepo _repo = repo;

        /// <summary>
        /// Retrieves all recovery exercises.
        /// </summary>
        /// <remarks>
        /// This endpoint allows an authenticated user to fetch all available recovery exercises.
        /// 
        /// Sample request:
        /// 
        ///     GET /api/RecoveryExercise
        /// 
        /// - A <c>200 OK</c> response is returned with a list of exercises.
        /// </remarks>
        /// <returns>A list of all recovery exercises.</returns>
        /// <response code="200">Returns a list of recovery exercises</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="401">If the user is not logged in</response>
        [HttpGet][Authorize]
        public async Task<IActionResult> GetAll() {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var result = await _repo.GetAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific recovery exercise by ID.
        /// </summary>
        /// <remarks>
        /// This endpoint allows a user to retrieve details of a specific recovery exercise by its ID.
        /// 
        /// Sample request:
        /// 
        ///     GET /api/RecoveryExercise/{id}
        ///     
        /// Replace <c>{id}</c> with the actual ID of the exercise.
        /// 
        /// - A <c>404 NotFound</c> response is returned if the exercise does not exist.
        /// - A <c>200 OK</c> response is returned with the exercise details if found.
        /// </remarks>
        /// <param name="id">The ID of the recovery exercise.</param>
        /// <returns>The specified recovery exercise.</returns>
        /// <response code="200">Returns the recovery exercise</response>
        /// <response code="404">If the exercise with the given <paramref name="id"/> was not found</response>
        /// <response code="401">If the user is not logged in</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id) {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var exercise = await _repo.GetByIdAsync(id);
            if (exercise == null) return NotFound();
            
            return Ok(exercise);
        }

        /// <summary>
        /// Creates a new recovery exercise.
        /// </summary>
        /// <remarks>
        /// This endpoint allows an authenticated user to create a new recovery exercise.
        /// 
        /// Sample request:
        /// 
        ///     POST /api/RecoveryExercise
        /// 
        /// Request body:
        /// 
        ///     {
        ///         
        ///         "name": "Stretching Exercise",
        ///         "description": "A basic stretching exercise for flexibility.",
        ///         "injuryId": "1"
        ///     }
        /// 
        /// - A <c>400 BadRequest</c> response is returned if the request data is invalid.
        /// - A <c>201 Created</c> response is returned with the created exercise details.
        /// </remarks>
        /// <param name="createDto">The details of the exercise to create.</param>
        /// <returns>The newly created recovery exercise.</returns>
        /// <response code="400">If the request data is invalid</response>
        /// <response code="201">If the recovery exercise is successfully created</response>
        /// <response code="401">If the user is not logged in</response>
        [HttpPost][Authorize]
        public async Task<IActionResult> Create([FromBody] CreateRecoveryExerciseDto createDto) {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var (exerciseDto, errorMessage) = await _repo.CreateAsync(createDto);

            if (exerciseDto == null) return BadRequest(errorMessage ?? "Failed to create recovery exercise.");

            return CreatedAtAction(nameof(GetById), new { id = exerciseDto.Id }, exerciseDto);
        }

        /// <summary>
        /// Updates a specific recovery exercise by ID.
        /// </summary>
        /// <remarks>
        /// This endpoint allows an authenticated user to update details of a recovery exercise by ID.
        /// 
        /// Sample request:
        /// 
        ///     PUT /api/RecoveryExercise/{id}
        ///     
        /// Replace <c>{id}</c> with the actual ID of the recovery exercise.
        /// 
        /// Request body:
        /// 
        ///     {
        ///         "name": "Updated Exercise Name",
        ///         "description": "Updated description.",
        ///         "injuryId": 2
        ///     }
        /// 
        /// - If the recovery exercise does not exist, a <c>404 NotFound</c> response is returned.
        /// - If the update is successful, a <c>204 NoContent</c> response is returned.
        /// </remarks>
        /// <param name="id">The ID of the recovery exercise to update.</param>
        /// <param name="updateDto">The updated details of the recovery exercise.</param>
        /// <returns>No content on successful update.</returns>
        /// <response code="400">If the request data is invalid</response>
        /// <response code="404">If the exercise with the given <paramref name="id"/> was not found</response>
        /// <response code="204">If the exercise is successfully updated</response>
        /// <response code="401">If the user is not logged in</response>
        [HttpPut("{id}")][Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRecoveryExerciseDto updateDto) {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var exists = await _repo.RecoveryExerciseExistsInDb(id);
            if (!exists) return NotFound();

            var (updatedExerciseDto, errorMessage) = await _repo.UpdateAsync(id, updateDto);
            if (updatedExerciseDto == null) return BadRequest(errorMessage); 

            return Ok(updatedExerciseDto);
        }

        [HttpDelete("{id}")][Authorize]
        public async Task<IActionResult> Delete(int id) {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var recoveryExercise = await _repo.DeleteAsync(id);
            if (recoveryExercise == null) return NotFound("Recovery exercise does not exist");
            return Ok(recoveryExercise);
        }
    }
}
