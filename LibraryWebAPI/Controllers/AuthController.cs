using LibraryShared.DTOs;
using LibraryWebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDTO>> Register(UserRegisterDTO userDto)
        {
            var result = await _authService.RegisterAsync(userDto);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDTO>> Login(UserLoginDTO userDto)
        {
            var result = await _authService.LoginAsync( userDto);
            return Ok(result);
        }
    }
}