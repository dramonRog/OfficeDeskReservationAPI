using OfficeDeskReservation.API.Dtos.Rooms;
using OfficeDeskReservation.API.Pagination;

namespace OfficeDeskReservation.API.Services.Interfaces
{
    public interface IRoomService
    {
        Task<PagedResult<RoomResponseDto>> GetAllRoomsAsync(QueryParameters queryParameters);
        Task<RoomResponseDto?> GetRoomByIdAsync(int id);
        Task<RoomResponseDto?> CreateRoomAsync(RoomDto roomDto);
        Task<bool> UpdateRoomAsync(int id, RoomDto roomDto);
        Task<bool> DeleteRoomAsync(int id);
    }
}
