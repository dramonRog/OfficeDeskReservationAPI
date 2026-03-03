using OfficeDeskReservation.API.Dtos.Reservations;

namespace OfficeDeskReservation.API.Services.Interfaces
{
    public interface IReservationService
    {
        Task<List<ReservationResponseDto>> GetAllReservationsAsync();
        Task<ReservationResponseDto?> GetReservationByIdAsync(int id);
        Task<ReservationResponseDto?> CreateReservationAsync(ReservationDto reservationDto);
        Task<bool> UpdateReservationAsync(int id, ReservationDto reservationDto);
        Task<bool> DeleteReservationAsync(int id);
    }
}
