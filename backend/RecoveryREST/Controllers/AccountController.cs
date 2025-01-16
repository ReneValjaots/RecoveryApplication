using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecoveryREST.Dtos.Account;
using RecoveryREST.Interfaces;
using RecoveryREST.Models.Classes;

namespace RecoveryREST.Controllers {
    [ApiController]
    [Route("api/account")]
    public class AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IDoctorTokenService doctorTokenService, SignInManager<AppUser> signInManager) : ControllerBase {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly SignInManager<AppUser> _signInManager = signInManager;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IDoctorTokenService _doctorTokenService = doctorTokenService;

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <remarks>
        /// This endpoint allows a new user to register by providing a username, email, and password.
        /// 
        /// Sample request:
        /// 
        ///     POST /api/account/register
        /// 
        /// Request body:
        /// 
        ///     {
        ///         "username": "Test123",
        ///         "email": "test123@example.com",
        ///         "password": "Test123!"
        ///     }
        /// 
        /// - A <c>200 OK</c> response is returned with the new user's details if the registration succeeds.
        /// - If the request data is invalid, a <c>400 BadRequest</c> response is returned.
        /// - If there is an internal server error, a <c>500 InternalServerError</c> response is returned.
        /// </remarks>
        /// <param name="registerDto">The registration details including username, email, and password.</param>
        /// <returns>The details of the newly created user, including a token.</returns>
        /// <response code="200">Returns the created user details along with a token</response>
        /// <response code="400">If the request data is invalid</response>
        /// <response code="500">If there is an error during the registration process</response>
        [HttpPost("register")] public async Task<IActionResult> Register([FromBody] RegisterDto registerDto) {
            try {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var appUser = new AppUser {
                    UserName = registerDto.Username,
                    Email = registerDto.Email,
                };

                var createdUser = await _userManager.CreateAsync(appUser, registerDto.Password);

                if (createdUser.Succeeded) {
                    var roleResult = await _userManager.AddToRoleAsync(appUser, "User");
                    if (roleResult.Succeeded) {
                        return Ok(new NewUserDto {
                            Username = appUser.UserName,
                            Email = appUser.Email,
                            Token = await _tokenService.CreateToken(appUser)
                        });
                    }
                    else {
                        return StatusCode(500, roleResult.Errors);
                    }
                }
                else {
                    return StatusCode(500, createdUser.Errors);
                }
            }
            catch (Exception ex) {
                return StatusCode(500, ex);
            }
        }

        [HttpPost("register/doctor")] public async Task<IActionResult> RegisterDoctor([FromBody] RegisterDoctorDto registerDto) {
            try {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                bool isDoctorRegistration = _doctorTokenService.IsValidDoctorSecretKey(registerDto.SecretKey);

                if (!isDoctorRegistration) return Unauthorized("Invalid doctor registration key");

                var appUser = new AppUser {
                    UserName = registerDto.Username,
                    Email = registerDto.Email,
                };

                var createdUser = await _userManager.CreateAsync(appUser, registerDto.Password);

                if (createdUser.Succeeded) {
                    var roleResult = await _userManager.AddToRoleAsync(appUser, "Doctor");
                    if (roleResult.Succeeded) {
                        return Ok(new NewUserDto {
                            Username = appUser.UserName,
                            Email = appUser.Email,
                            Token = await _doctorTokenService.CreateToken(appUser)
                        });
                    }
                    else {
                        return StatusCode(500, roleResult.Errors);
                    }
                }
                else {
                    return StatusCode(500, createdUser.Errors);
                }
            }
            catch (Exception ex) {
                return StatusCode(500, ex);
            }
        }


        /// <summary>
        /// Logs in a user.
        /// </summary>
        /// <remarks>
        /// This endpoint allows an existing user to log in by providing their username and password.
        /// 
        /// Sample request:
        /// 
        ///     POST /api/account/login
        /// 
        /// Request body:
        /// 
        ///     {
        ///         "username": "Test123",
        ///         "password": "Test123!"
        ///     }
        /// 
        /// - A <c>200 OK</c> response is returned with the user's details and a token if login is successful.
        /// - If the username or password is incorrect, a <c>401 Unauthorized</c> response is returned.
        /// - If the request data is invalid, a <c>400 BadRequest</c> response is returned.
        /// </remarks>
        /// <param name="loginDto">The login details including username and password.</param>
        /// <returns>The logged-in user's details, including a token.</returns>
        /// <response code="200">Returns the user details along with a token</response>
        /// <response code="400">If the request data is invalid</response>
        /// <response code="401">If the username or password is incorrect</response>
        [HttpPost("login")] public async Task<IActionResult> Login(LoginDto loginDto) {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.Users
                .FirstOrDefaultAsync(x => x.UserName.ToLower() == loginDto.Username.ToLower() || x.Email.ToLower() == loginDto.Email.ToLower());

            if (user == null) return Unauthorized("Invalid username");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded) return Unauthorized("Username or password incorrect or not found");

            return Ok(new NewUserDto
            {
                Username = user.UserName,
                Email = user.Email,
                Token = await _tokenService.CreateToken(user)
            });
        }
    }
}
