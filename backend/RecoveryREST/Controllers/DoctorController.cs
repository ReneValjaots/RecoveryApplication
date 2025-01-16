using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecoveryREST.Data;
using RecoveryREST.Dtos.Doctor;
using RecoveryREST.Dtos.RecoveryPlan;
using RecoveryREST.Extensions;
using RecoveryREST.Interfaces;
using RecoveryREST.Models.Classes;

namespace RecoveryREST.Controllers {
    [ApiController]
    [Route("api/doctor")]
    public class DoctorController(ApplicationDbContext context, UserManager<AppUser> userManager, IDoctorRepo repo) : ControllerBase {
        private readonly ApplicationDbContext _context = context;
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly IDoctorRepo _repo = repo;

        [HttpGet("injuries")][Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetAllPatients() {
            var severeInjuries = await _repo.GetAllPatientsAsync();

            if (severeInjuries == null || severeInjuries.Count == 0) return NotFound("No severe injuries found.");
            
            return Ok(severeInjuries);
        }
        
        [HttpGet("patients/available")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetAllAvailablePatients() {
            var patients = await _repo.GetAllAvailablePatientsAsync();

            if (patients.Count == 0 || patients == null) return NotFound("No patients with severe injuries and no assigned doctor found.");

            return Ok(patients);
        }

        [HttpPatch("assign-doctor")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> AssignDoctorToInjury([FromBody] AssignDoctorDto assignDto) {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userInjury = await _repo.GetUserInjuryAsync(assignDto.AppUserId, assignDto.InjuryId);

            if (userInjury == null)
                return NotFound($"Injury with ID {assignDto.InjuryId} not found for this user.");

            if (!userInjury.IsTooSevere)
                return BadRequest("This injury is not severe enough to assign a doctor.");

            var username = User.GetUsername();
            var doctor = await _userManager.FindByNameAsync(username);
            if (doctor == null) return Unauthorized("Doctor not found");
            var doctorId = doctor.Id; 

            await _repo.AssignDoctorToInjury(userInjury, doctorId);

            return Ok($"Doctor assigned to the injury successfully.");
        }

        [HttpGet("patients")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetAllDoctorPatients() {
            var username = User.GetUsername();
            var appUser = await _userManager.FindByNameAsync(username);
            var doctorId = appUser.Id; 

            var patients = await _repo.GetAllDoctorPatientsAsync(doctorId);

            if (patients.Count == 0) return NotFound("No patients assigned to this doctor.");

            return Ok(patients);
        }

        [HttpGet("recovery-plans")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetDoctorCreatedRecoveryPlans() {
            var username = User.GetUsername();
            var doctor = await _userManager.FindByNameAsync(username);
            if (doctor == null) return Unauthorized("Doctor not found"); 

            var recoveryPlans = await _repo.GetAllRecoveryPlansAsync(doctor.Id);

            return Ok(recoveryPlans);
        }

        [HttpPost("create-plan")][Authorize(Roles = "Doctor")]
        public async Task<IActionResult> CreateRecoveryPlan([FromBody] CreateRecoveryPlanDto dto) {
            if (!ModelState.IsValid) return BadRequest(ModelState);  

            var username = User.GetUsername();
            var doctor = await _userManager.FindByNameAsync(username);
            if (doctor == null) return Unauthorized("Doctor not found");

            var isLinked = await _repo.IsDoctorLinkedToUserAsync(doctor.Id, dto.AppUserId);
            if (!isLinked) return Forbid();

            var (recoveryPlan, errorMessage) = await _repo.CreateRecoveryPlanAsync(dto, doctor.Id);
            if (recoveryPlan == null) return BadRequest(errorMessage ?? "Failed to create the recovery plan.");

            return CreatedAtAction(nameof(GetRecoveryPlanById), new { id = recoveryPlan.Id }, recoveryPlan);
        }

        [HttpGet("recovery-plan/{id}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetRecoveryPlanById(int id) {
            var username = User.GetUsername();
            var doctor = await _userManager.FindByNameAsync(username);
            if (doctor == null) return Unauthorized("Doctor not found");

            var result = await _repo.GetRecoveryPlanByIdAsync(id, doctor.Id);
            if (result == null) return NotFound($"Recovery plan with ID {id} not found.");
            return Ok(result);
        }

        [HttpPut("update-plan/{id}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> UpdateRecoveryPlan(int id, [FromBody] DoctorUpdatePlanDto dto) {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var username = User.GetUsername();
            var doctor = await _userManager.FindByNameAsync(username);
            if (doctor == null) return Unauthorized("Doctor not found");

            var (updatedPlan, errorMessage) = await _repo.UpdateRecoveryPlanAsync(id, dto, doctor.Id);
            if (updatedPlan == null) return BadRequest(errorMessage ?? "Failed to update the recovery plan.");

            return Ok(updatedPlan);
        }

        [HttpDelete("unassign-doctor")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> UnassignDoctorFromPatient([FromBody] AssignDoctorDto unassignDto) {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var username = User.GetUsername();
            var doctor = await _userManager.FindByNameAsync(username);
            if (doctor == null) return Unauthorized("Doctor not found");

            var userInjury = await _repo.GetUserInjuryAsync(unassignDto.AppUserId, unassignDto.InjuryId);

            if (userInjury == null)
                return NotFound($"Injury with ID {unassignDto.InjuryId} not found for this user.");

            if (userInjury.DoctorId != doctor.Id)
                return Unauthorized("You are not the assigned doctor for this injury.");

            await _repo.UnassignDoctorFromInjuryAsync(userInjury);

            return Ok("Doctor unassigned from the injury successfully.");
        }
        
        [HttpDelete("delete-plan/{id}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> DeleteRecoveryPlan(int id) {
            var username = User.GetUsername();
            var doctor = await _userManager.FindByNameAsync(username);
            if (doctor == null) return Unauthorized("Doctor not found");

            try {
                await _repo.DeleteRecoveryPlanAsync(id, doctor.Id);
                return Ok($"Recovery plan with ID {id} has been deleted successfully.");
            } catch (InvalidOperationException ex) {
                return NotFound(ex.Message);
            }
        }
    }
}