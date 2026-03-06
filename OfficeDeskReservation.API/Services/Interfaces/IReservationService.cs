using OfficeDeskReservation.API.Dtos.Reservations;
using OfficeDeskReservation.API.Pagination;

namespace OfficeDeskReservation.API.Services.Interfaces
{
    public interface IReservationService
    {
        Task<PagedResult<ReservationResponseDto>> GetAllReservationsAsync(ReservationQueryParameters queryParameters);
        Task<ReservationResponseDto?> GetReservationByIdAsync(int id);
        Task<ReservationResponseDto?> CreateReservationAsync(ReservationDto reservationDto, int userId);
        Task<bool> UpdateReservationAsync(int id, ReservationDto reservationDto);
        Task<bool> DeleteReservationAsync(int id);
    }
}
