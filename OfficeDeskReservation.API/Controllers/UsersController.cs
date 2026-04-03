using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeDeskReservation.API.Dtos.Users;
using OfficeDeskReservation.API.Pagination;
using OfficeDeskReservation.API.Services.Interfaces;
using System.Security.Claims;

namespace OfficeDeskReservation.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;
        public UsersController(IUserService service)
        {
            _service = service;
        }


        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public async Task<ActionResult<PagedResult<UserResponseDto>>> GetUsersAsync([FromQuery] UserQueryParameters queryParameters)
        {
            PagedResult<UserResponseDto> users = await _service.GetUsersAsync(queryParameters);
            return Ok(users);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUserByIdAsync(int id)
        {
            UserResponseDto? user = await _service.GetUserByIdAsync(id);

            if (user == null)
                return NotFound();
            return Ok(user);
        }


        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserAsync(int id, [FromBody] UserDto user)
        {
            string? currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(currentUserId, out int currentId))
                return Unauthorized();

            if (id != currentId && !User.IsInRole("Admin"))
                return Forbid();

            if (await _service.UpdateUserAsync(id, user))
                return NoContent();
            return NotFound();
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/role")]
        public async Task<IActionResult> ChangeRoleAsync(int id, [FromBody] ChangeRoleDto request)
        {
            if (await _service.ChangeRoleAsync(id, request))
                return NoContent();
            return NotFound();
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveUserByIdAsync(int id)
        {
            string? currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(currentUserId, out int currentId))
                return Unauthorized();

            if (currentId == id)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Action Not Allowed",
                    Detail = "You cannot delete your own account"
                });
            }

            if (await _service.DeleteUserAsync(id))
                return NoContent();
            return NotFound();
        }
    }
}
