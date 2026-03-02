using OfficeDeskReservation.API.Dtos.Users;

namespace OfficeDeskReservation.API.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<UserResponseDto>> GetUsersAsync();
        Task<UserResponseDto?> GetUserByIdAsync(int id);
        Task<UserResponseDto?> CreateUserAsync(UserDto userDto);
        Task<bool> UpdateUserAsync(int id, UserDto userDto);
        Task<bool> DeleteUserAsync(int id);
    }
}
