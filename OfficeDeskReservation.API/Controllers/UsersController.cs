using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeDeskReservation.API.Data;
using OfficeDeskReservation.API.Dtos.Users;
using OfficeDeskReservation.API.Models;

namespace OfficeDeskReservation.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public UsersController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<ActionResult<List<UserResponseDto>>> GetUsersAsync()
        {
            List<User> usersList = await _context.Users
                .Include(u => u.Reservations)
                    .ThenInclude(r => r.Desk)
                        .ThenInclude(d => d.Room)
            .ToListAsync();

            return Ok(_mapper.Map<List<UserResponseDto>>(usersList));
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUserByIdAsync(int id)
        {
            User? user = await _context.Users
                .Include(u => u.Reservations)
                    .ThenInclude(r => r.Desk)
                        .ThenInclude(d => d.Room)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();
            return Ok(_mapper.Map<UserResponseDto>(user));
        }


        [HttpPost]
        public async Task<ActionResult<UserResponseDto>> PostUserAsync([FromBody] UserDto user)
        {
            if (await _context.Users.AnyAsync(u => u.FirstName == user.FirstName && u.LastName == user.LastName))
                return Conflict("User with such name and surname already exists.");
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                return Conflict("User with such email already exists.");

            User result = _mapper.Map<User>(user);
            _context.Users.Add(result);
            await _context.SaveChangesAsync();
            
            UserResponseDto response = _mapper.Map<UserResponseDto>(result);

            return CreatedAtAction("GetUserById", new { id = response.Id }, response);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserAsync(int id, [FromBody] UserDto user)
        {
            User? existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (existingUser == null)
                return NotFound("No user with such ID.");

            if (await _context.Users.AnyAsync(u => u.Email == user.Email && u.Id != id))
                return Conflict("Another user with this email already exists.");

            if (await _context.Users.AnyAsync(u => u.FirstName == user.FirstName && u.LastName == user.LastName && u.Id != id))
                return Conflict("Another user with such name and surname already exists.");

            _mapper.Map(user, existingUser);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveUserByIdAsync(int id)
        {
            User? user = await _context.Users.Include(u => u.Reservations).FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            _context.Reservations.RemoveRange(user.Reservations);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
