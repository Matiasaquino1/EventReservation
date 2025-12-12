using AutoMapper;
using EventReservations.Models;  // Para los modelos
using EventReservations.Dto;   // Para los DTOs

namespace EventReservations.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // EVENTS
            CreateMap<Event, EventDto>();
            CreateMap<CreateEventDto, Event>()
                .ForMember(dest => dest.TicketsAvailable, opt => opt.Ignore())
                .ForMember(dest => dest.TotalTickets, opt => opt.MapFrom(src => src.TotalTickets))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.EventId, opt => opt.Ignore());
            CreateMap<UpdateEventDto, Event>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Event, UpdateEventDto>();
                
            // RESERVATIONS
            CreateMap<Reservation, ReservationDto>()
                .ForMember(dest => dest.ReservationId, opt => opt.MapFrom(src => src.ReservationId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.EventId, opt => opt.MapFrom(src => src.EventId))
                .ReverseMap();

            CreateMap<CreatedReservationDto, Reservation>();

            // DTO para Admin (incluye user email)
            CreateMap<Reservation, AdminReservationDto>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.ReservationId, opt => opt.MapFrom(src => src.ReservationId))
                .ForMember(dest => dest.EventId, opt => opt.MapFrom(src => src.EventId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));

            CreateMap<User, LoginResponseDto>();
            CreateMap<User, UserDto>();

            // PAYMENTS
            CreateMap<Payment, PaymentRequestDto>().ReverseMap();
            CreateMap<Payment, PaymentDto>().ReverseMap();

        }
    }

}