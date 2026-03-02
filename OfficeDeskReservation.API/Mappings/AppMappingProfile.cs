using AutoMapper;
using OfficeDeskReservation.API.Dtos.Desks;
using OfficeDeskReservation.API.Dtos.Reservations;
using OfficeDeskReservation.API.Dtos.Rooms;
using OfficeDeskReservation.API.Dtos.Users;
using OfficeDeskReservation.API.Models;

namespace OfficeDeskReservation.API.Mappings
{
    public class AppMappingProfile : Profile
    {
        public AppMappingProfile()
        {
            CreateMap<DeskDto,Desk>();
            CreateMap<Desk, DeskResponseDto>()
                .ForMember(dest => dest.RoomName,
                           opt => opt.MapFrom(src => src.Room!.Name));

            CreateMap<RoomDto, Room>();
            CreateMap<Room, RoomResponseDto>();

            CreateMap<UserDto, User>();
            CreateMap<User, UserResponseDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

            CreateMap<ReservationDto, Reservation>();
            CreateMap<Reservation, ReservationResponseDto>()
                .ForMember(dest => dest.UserName,
                           opt => opt.MapFrom(src => $"{src.User!.FirstName} {src.User.LastName}"))
                .ForMember(dest => dest.DeskName,
                           opt => opt.MapFrom(src => src.Desk!.DeskIdentifier))
                .ForMember(dest => dest.RoomName,
                           opt => opt.MapFrom(src => src.Desk!.Room!.Name));
        }
    }
}
