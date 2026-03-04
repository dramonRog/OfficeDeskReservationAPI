using OfficeDeskReservation.API.Dtos.Users;
using OfficeDeskReservation.API.Dtos.Auth;

namespace OfficeDeskReservation.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<UserResponseDto> RegisterAsync(RegisterDto request);
        Task<string> LoginAsync(LoginDto request);
    }
}
