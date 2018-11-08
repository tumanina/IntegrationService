using System.Collections.Generic;
using AutoMapper;
using Price = IntegrationService.Business.DTO.Price;
using Product = IntegrationService.Business.DTO.Product;

namespace IntegrationService.API.Areas.V1
{
    internal class AutoMapperProfile : Profile
    {
        protected override void Configure()
        {
            CreateMap<Product, Models.Product>();

            Mapper.AssertConfigurationIsValid();
        }
    }
}