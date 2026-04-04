using AutoMapper;
using ShopAPI.Data.Entities;
using ShopAPI.DTO;

namespace ShopAPI.Profiles
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<ProductRequestDTO, Product>();

            CreateMap<Product, ProductResponseDTO>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.Tax, opt => opt.MapFrom(src => src.TaxLevel));

            CreateMap<Category, ProductCategoryDTO>();
            CreateMap<Tax, ProductTaxDTO>();
        }
    }
}