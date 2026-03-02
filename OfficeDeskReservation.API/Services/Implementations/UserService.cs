using AutoMapper;
using OfficeDeskReservation.API.Data;
using OfficeDeskReservation.API.Dtos.Users;
using OfficeDeskReservation.API.Services.Interfaces;
using OfficeDeskReservation.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace OfficeDeskReservation.API.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public UserService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<UserResponseDto>> GetUsersAsync()
        {
            List<User> users = await _context.Users
                .Include(u => u.Reservations)
                    .ThenInclude(r => r.Desk)
                        .ThenInclude(d => d.Room)
                .ToListAsync();

            return _mapper.Map<List<UserResponseDto>>(users);
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(int id)
        {
            User? user = await _context.Users
                .Include(u => u.Reservations)
                    .ThenInclude(r => r.Desk)
                        .ThenInclude(d => d.Room)
                .FirstOrDefaultAsync(u => u.Id == id);

            return user == null ? null : _mapper.Map<UserResponseDto>(user);   
        }

        public async Task<UserResponseDto?> CreateUserAsync(UserDto user)
        {
            if (await _context.Users.AnyAsync(u => u.FirstName == user.FirstName && u.LastName == user.LastName))
                throw new InvalidOperationException("This user already exists.");

            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                throw new InvalidOperationException("User with such email already exists.");

            User newUser = _mapper.Map<User>(user);
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return _mapper.Map<UserResponseDto>(newUser);
        }

        public async Task<bool> UpdateUserAsync(int id, UserDto user)
        {
            User? existingUser = await _context.Users.Include(u => u.Reservations).FirstOrDefaultAsync(u => u.Id == id);

            if (existingUser == null)
                return false;

            if (await _context.Users.AnyAsync(u => u.Email == user.Email && u.Id != existingUser.Id))
                throw new InvalidOperationException("Another user with this email already exists.");

            if (await _context.Users.AnyAsync(u => u.FirstName == user.FirstName && u.LastName == user.LastName && u.Id != id))
                throw new InvalidOperationException("The user with such name and surname already exists.");

            _mapper.Map(user, existingUser);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            User? user = await _context.Users.Include(u => u.Reservations).FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return false;

            _context.Reservations.RemoveRange(user.Reservations);
            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
