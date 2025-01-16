using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecoveryREST.Dtos.Injury;
using RecoveryREST.Interfaces;

namespace RecoveryREST.Controllers {
    [ApiController]
    [Route("api/injury")]
    public class InjuryController(IInjuryRepo repo) : ControllerBase {
        private readonly IInjuryRepo _repo = repo;

        /// <summary>
        /// Retrieves all injuries.
        /// </summary>
        /// <remarks>
        /// This endpoint allows an authenticated user to retrieve a list of all injuries.
        /// 
        /// Sample request:
        /// 
        ///     GET /api/Injury
        /// 
        /// - A <c>200 OK</c> response is returned with a list of injuries.
        /// </remarks>
        /// <returns>A list of all injuries.</returns>
        /// <response code="200">Returns a list of injuries</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="401">If the user is not logged in</response>
        [HttpGet][Authorize]
        public async Task<IActionResult> GetAll() {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _repo.GetAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific injury by ID.
        /// </summary>
        /// <remarks>
        /// This endpoint allows a user to retrieve details of a specific injury by its ID.
        /// 
        /// Sample request:
        /// 
        ///     GET /api/Injury/{id}
        ///     
        /// Replace <c>{id}</c> with the actual ID of the injury.
        /// 
        /// - A <c>404 NotFound</c> response is returned if the injury does not exist.
        /// - A <c>200 OK</c> response is returned with the injury details if found.
        /// </remarks>
        /// <param name="id">The ID of the injury.</param>
        /// <returns>The specified injury.</returns>
        /// <response code="200">Returns the injury</response>
        /// <response code="404">If the injury with the given <paramref name="id"/> was not found</response>
        /// <response code="401">If the user is not logged in</response>
        [HttpGet("{id}")][Authorize]
        public async Task<IActionResult> GetById(int id) {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var injury = await _repo.GetByIdAsync(id);
            if (injury == null) return NotFound();
            
            return Ok(injury);
        }

        /// <summary>
        /// Creates a new injury.
        /// </summary>
        /// <remarks>
        /// This endpoint allows an authenticated user to create a new injury record.
        /// 
        /// Sample request:
        /// 
        ///     POST /api/Injury
        /// 
        /// Request body:
        /// 
        ///     {
        ///         "name": "Knee Injury",
        ///         "description": "Injury description...",
        ///         "recoveryExerciseId": 1
        ///     }
        /// 
        /// - A <c>400 BadRequest</c> response is returned if the request data is invalid.
        /// - A <c>201 Created</c> response is returned with the created injury details.
        /// </remarks>
        /// <param name="createDto">The details of the injury to create.</param>
        /// <returns>The newly created injury.</returns>
        /// <response code="400">If the request data is invalid</response>
        /// <response code="201">If the injury is successfully created</response>
        /// <response code="401">If the user is not logged in</response>
        [HttpPost][Authorize]
        public async Task<IActionResult> Create([FromBody] CreateInjuryDto createDto) {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var (injuryDto, errorMessage) = await _repo.CreateAsync(createDto);

            if (injuryDto == null) return BadRequest(errorMessage ?? "Failed to create injury.");
            
            return CreatedAtAction(nameof(GetById), new { id = injuryDto.Id }, injuryDto);
        }

        /// <summary>
        /// Updates a specific injury by ID.
        /// </summary>
        /// <remarks>
        /// This endpoint allows an authenticated user to update details of an injury by its ID.
        /// 
        /// Sample request:
        /// 
        ///     PUT /api/Injury/{id}
        ///     
        /// Replace <c>{id}</c> with the actual ID of the injury.
        /// 
        /// Request body:
        /// 
        ///     {
        ///         "name": "Updated Injury Name",
        ///         "description": "Updated description.",
        ///         "recoveryExerciseId": 1
        ///     }
        /// 
        /// - If the injury does not exist, a <c>404 NotFound</c> response is returned.
        /// - If the update is successful, a <c>204 NoContent</c> response is returned.
        /// </remarks>
        /// <param name="id">The ID of the injury to update.</param>
        /// <param name="updateDto">The updated details of the injury.</param>
        /// <returns>No content on successful update.</returns>
        /// <response code="400">If the request data is invalid</response>
        /// <response code="404">If the injury with the given <paramref name="id"/> was not found</response>
        /// <response code="204">If the injury is successfully updated</response>
        /// <response code="401">If the user is not logged in</response>
        [HttpPut("{id}")][Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateInjuryDto updateDto) {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var exists = await _repo.InjuryExistsInDb(id);
            if (!exists) return NotFound();

            var (updatedInjuryDto, errorMessage) = await _repo.UpdateAsync(id, updateDto);
            if (updatedInjuryDto == null) return BadRequest(errorMessage);

            return Ok(updatedInjuryDto);
        }

        [HttpDelete("{id}")][Authorize]
        public async Task<IActionResult> Delete(int id) {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var injury = await _repo.DeleteAsync(id);
            if (injury == null) return NotFound("Injury does not exist");
            return Ok(injury);
        }
    }
}
