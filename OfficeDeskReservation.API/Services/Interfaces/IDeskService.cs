using OfficeDeskReservation.API.Dtos.Desks;

namespace OfficeDeskReservation.API.Services.Interfaces
{
    public interface IDeskService
    {
        Task<List<DeskResponseDto>> GetDesksAsync();
        Task<DeskResponseDto?> GetDeskByIdAsync(int id);
        Task<DeskResponseDto?> CreateDeskAsync(DeskDto desk);
        Task<bool> UpdateDeskAsync(int id, DeskDto desk);
        Task<bool> DeleteDeskAsync(int id);
    }
}
