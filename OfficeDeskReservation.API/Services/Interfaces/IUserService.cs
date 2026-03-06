using OfficeDeskReservation.API.Dtos.Users;
using OfficeDeskReservation.API.Pagination;

namespace OfficeDeskReservation.API.Services.Interfaces
{
    public interface IUserService
    {
        Task<PagedResult<UserResponseDto>> GetUsersAsync(QueryParameters queryParameters);
        Task<UserResponseDto?> GetUserByIdAsync(int id);
        Task<bool> UpdateUserAsync(int id, UserDto userDto);
        Task<bool> DeleteUserAsync(int id);
    }
}
