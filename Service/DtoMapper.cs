using AutoMapper;
using Core.DTOs;
using Core.Entities;

namespace Service;

public class DtoMapper :Profile
{
    public DtoMapper() {
        CreateMap<ProductDto, Product>().ReverseMap();
        CreateMap<UserDto, User>().ReverseMap();
    }
    
}