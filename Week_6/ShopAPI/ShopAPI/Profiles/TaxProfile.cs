using AutoMapper;
using ShopAPI.Data.Entities;
using ShopAPI.DTO;

namespace ShopAPI.Profiles
{
    public class TaxProfile : Profile
    {
        public TaxProfile()
        {
            CreateMap<TaxRequestDTO, Tax>();
            CreateMap<Tax, TaxResponseDTO>();
        }
    }
}