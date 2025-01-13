using App.Contract.Dto;
using App.Core.Entites;
using AutoMapper;

namespace App.Services.Helpers
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<AspNetUser, UserProfile>()
                 .ForMember(dest => dest.PhoneNumber, member => member.MapFrom(i => i.PhoneNumber))
                 .ForMember(dest => dest.Email, member => member.MapFrom(i => i.UserName))
                 .ForMember(dest => dest.CountryCode, member => member.MapFrom(i => i.CountryCode))
                 .ForMember(dest => dest.UserId, member => member.MapFrom(i => i.Id));
        }
    }
}
