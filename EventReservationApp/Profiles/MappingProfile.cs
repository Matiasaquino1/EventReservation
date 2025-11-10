using AutoMapper;
using EventReservations.Models;  // Para los modelos
using EventReservations.Dto;   // Para los DTOs

namespace EventReservations.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mapeos para Events
            CreateMap<Event, EventDto>();  // De modelo a DTO
            CreateMap<CreateEventDto, Event>();  // De DTO a modelo
            CreateMap<UpdateEventDto, Event>();  // De DTO a modelo
            CreateMap<Event, UpdateEventDto>().ReverseMap();  // Bidireccional si es necesario
            // Mapeos para Reservations
            CreateMap<Reservation, ReservationDto>()
                .ForMember(dest => dest.ReservationId, opt => opt.MapFrom(src => src.ReservationId))
                .ReverseMap()
                .ForMember(dest => dest.ReservationId, opt => opt.MapFrom(src => src.ReservationId));
            CreateMap<CreatedReservationDto, Reservation>();
            CreateMap<Reservation, AdminReservationDto>();  // Si necesitas mapear a DTO admin
            // Mapeos para Payments (si tienes más DTOs)
            CreateMap<Payment, PaymentRequestDto>().ReverseMap();  // Asume que tienes PaymentDto
        }
    }
}