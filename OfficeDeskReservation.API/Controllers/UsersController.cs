using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeDeskReservation.API.Data;
using OfficeDeskReservation.API.Dtos;
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
        public async Task<ActionResult<List<UserDto>>> GetUsersAsync()
        {
            List<User> usersList = await _context.Users.ToListAsync();
            return Ok(_mapper.Map<List<UserDto>>(usersList));
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserByIdAsync(int id)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();
            return _mapper.Map<UserDto>(user);
        }


        [HttpPost]
        public async Task<ActionResult<UserDto>> PostUserAsync([FromBody] UserDto user)
        {
            if (await _context.Users.AnyAsync(u => u.FirstName == user.FirstName && u.LastName == user.LastName))
                return Conflict("User with such name and surname already exists.");
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                return Conflict("User with such email already exists.");

            User result = _mapper.Map<User>(user);
            _context.Add(result);
            await _context.SaveChangesAsync();
            _mapper.Map(result, user);

            return CreatedAtAction("GetUserById", new { id = user.Id }, user);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserAsync(int id, [FromBody] UserDto user)
        {
            if (id != user.Id)
                return BadRequest("ID in URL doesn't match ID in body");

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
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
