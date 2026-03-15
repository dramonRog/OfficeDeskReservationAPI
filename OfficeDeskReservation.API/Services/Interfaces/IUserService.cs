using OfficeDeskReservation.API.Dtos.Users;
using OfficeDeskReservation.API.Pagination;

namespace OfficeDeskReservation.API.Services.Interfaces
{
    public interface IUserService
    {
        Task<PagedResult<UserResponseDto>> GetUsersAsync(UserQueryParameters queryParameters);
        Task<UserResponseDto?> GetUserByIdAsync(int id);
        Task<bool> UpdateUserAsync(int id, UserDto userDto);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> ChangeRoleAsync(int id, ChangeRoleDto reqest);
    }
}
