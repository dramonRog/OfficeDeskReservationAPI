using Microsoft.AspNetCore.Mvc;
using OfficeDeskReservation.API.Dtos.Auth;
using OfficeDeskReservation.API.Dtos.Users;
using OfficeDeskReservation.API.Services.Interfaces;

namespace OfficeDeskReservation.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;

        public AuthController(IAuthService service)
        {
            _service = service;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserResponseDto>> RegisterAsync([FromBody] RegisterDto request)
        {
            UserResponseDto response = await _service.RegisterAsync(request);
            return Ok(response);
        }


        [HttpPost("login")]
        public async Task<ActionResult<string>> LoginAsync([FromBody] LoginDto request)
        {
            string response = await _service.LoginAsync(request);
            return Ok(new { Token = response });
        }
    }
}
