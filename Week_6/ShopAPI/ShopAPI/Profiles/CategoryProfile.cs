using AutoMapper;
using ShopAPI.Data.Entities;
using ShopAPI.DTO;

namespace ShopAPI.Profiles
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<CategoryRequestDTO, Category>();
            CreateMap<Category, CategoryResponseDTO>();
        }
    }
}