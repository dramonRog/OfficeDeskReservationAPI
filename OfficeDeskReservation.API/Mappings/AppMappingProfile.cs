using AutoMapper;
using OfficeDeskReservation.API.Dtos;
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

            CreateMap<Room, RoomDto>();
            CreateMap<RoomDto, Room>();

            CreateMap<UserDto, User>();
            CreateMap<User, UserDto>();

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
