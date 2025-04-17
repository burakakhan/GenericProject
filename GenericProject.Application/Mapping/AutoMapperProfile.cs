using AutoMapper;
using GenericProject.Application.DTOs;
using GenericProject.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericProject.Application.Mapping
{
    public class AutoMapperProfile:Profile
    {
        public AutoMapperProfile()
        {
            // Product -> ProductDto ve tersi
            CreateMap<Product, ProductDto>().ReverseMap();

            // CreateProductDto -> Product
            CreateMap<CreateProductDto, Product>()
                // Id ve tarihler DTO'da yok, maplenmemeli
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore());
                // Yeni ürün varsayılan olarak Active olsun
                //.ForMember(dest => dest.Status, opt => opt.MapFrom(src => Contracts.Enums.ProductStatus.Active));


            // UpdateProductDto -> Product
            CreateMap<UpdateProductDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // Id güncellenmemeli
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore()); // Oluşturma tarihi güncellenmemeli
                                                                           // UpdatedDate UnitOfWork içinde veya burada ayarlanabilir
                                                                           // .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow)); // Veya UoW'da
        }
    }
}
