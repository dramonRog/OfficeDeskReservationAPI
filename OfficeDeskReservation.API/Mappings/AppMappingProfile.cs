using AutoMapper;
using OfficeDeskReservation.API.Dtos;
using OfficeDeskReservation.API.Models;

namespace OfficeDeskReservation.API.Mappings
{
    public class AppMappingProfile : Profile
    {
        public AppMappingProfile()
        {
            CreateMap<Desk, DeskDto>();
            CreateMap<Room, RoomDto>();

            CreateMap<DeskDto, Desk>();
            CreateMap<RoomDto, Room>();

            CreateMap<UserDto, User>();
            CreateMap<User, UserDto>();
        }
    }
}
