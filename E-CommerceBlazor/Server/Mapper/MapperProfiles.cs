﻿using AutoMapper;
using E_CommerceBlazor.Server.Dto;
using E_CommerceBlazor.Server.Model;

namespace E_CommerceBlazor.Server.Service.Mapper
{
    public class MapperProfiles : Profile
    {
        public MapperProfiles()
        {
            CreateMap<RegisterDto,User>();
            CreateMap<ProductCreateDto, Product>();
            
            CreateMap<Product, ProductReadDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));
        
            CreateMap<Category,CategoryDto>();
        }
    }
}