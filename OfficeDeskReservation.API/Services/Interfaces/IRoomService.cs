using OfficeDeskReservation.API.Dtos.Rooms;

namespace OfficeDeskReservation.API.Services.Interfaces
{
    public interface IRoomService
    {
        Task<List<RoomResponseDto>> GetAllRoomsAsync();
        Task<RoomResponseDto?> GetRoomByIdAsync(int id);
        Task<RoomResponseDto?> CreateRoomAsync(RoomDto roomDto);
        Task<bool> UpdateRoomAsync(int id, RoomDto roomDto);
        Task<bool> DeleteRoomAsync(int id);
    }
}
