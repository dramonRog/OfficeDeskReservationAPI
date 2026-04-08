using AutoMapper;
using OfficeDeskReservation.API.Data;
using OfficeDeskReservation.API.Dtos.Users;
using OfficeDeskReservation.API.Services.Interfaces;
using OfficeDeskReservation.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OfficeDeskReservation.API.Pagination;

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

        public async Task<PagedResult<UserResponseDto>> GetUsersAsync(UserQueryParameters queryParameters)
        {
            var query = _context.Users
                .Include(u => u.Reservations)
                    .ThenInclude(r => r.Desk)
                        .ThenInclude(d => d.Room)
                .AsQueryable();

            if (queryParameters.FirstNameTerm != null)
                query = query.Where(u => u.FirstName.StartsWith(queryParameters.FirstNameTerm));

            if (queryParameters.LastNameTerm != null)
                query = query.Where(u => u.LastName.StartsWith(queryParameters.LastNameTerm));

            if (queryParameters.EmailTerm != null)
                query = query.Where(u => u.Email.StartsWith(queryParameters.EmailTerm));

            int totalCount = await query.CountAsync();

            List<User> users = await query
                .Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
                .Take(queryParameters.PageSize)
                .ToListAsync();

            return new PagedResult<UserResponseDto> 
            { 
                Items = _mapper.Map<List<UserResponseDto>>(users),
                TotalCount = totalCount,
                PageNumber = queryParameters.PageNumber,
                PageSize = queryParameters.PageSize
            };
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

        public async Task<bool> ChangeRoleAsync(int id, ChangeRoleDto request)
        {
            User? user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            user.Role = request.Role;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto request)
        {
            User? currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (currentUser == null)
                return false;

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, currentUser.PasswordHash))
                return false;

            currentUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
