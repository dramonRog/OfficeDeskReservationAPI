using OfficeDeskReservation.API.Dtos.Desks;
using OfficeDeskReservation.API.Pagination;

namespace OfficeDeskReservation.API.Services.Interfaces
{
    public interface IDeskService
    {
        Task<PagedResult<DeskResponseDto>> GetDesksAsync(DeskQueryParameters queryParameters);
        Task<DeskResponseDto?> GetDeskByIdAsync(int id);
        Task<DeskResponseDto?> CreateDeskAsync(DeskDto desk);
        Task<bool> UpdateDeskAsync(int id, DeskDto desk);
        Task<bool> DeleteDeskAsync(int id);
    }
}
