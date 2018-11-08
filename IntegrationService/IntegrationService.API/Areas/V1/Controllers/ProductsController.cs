using System;
using System.Threading;
using System.Web.Http;
using AutoMapper;
using IntegrationService.Business;
using ApacKernel.AspNet.WebApi.Attributes;
using IntegrationService.Business.DTO;
using Product = IntegrationService.Business.DTO.Product;


namespace IntegrationService.API.Areas.V1.Controllers
{

    [ApiKeyAuthentication]
    [RoutePrefix(VersionSettings.V1)]
    [ApiKeyAuthentication]
    public class ProductController : ApiController
    {
        private readonly IProductService _service;

        public ProductController(IProductService service)
        {
            _service = service;
        }

        [Route("products/{id}")]
        [HttpGet]
        public IHttpActionResult Get(string id)
        {
            try
            {
                var keys = id.Split('_');
                if (keys.Length != 3)
                {
                    return BadRequest("Key has invalid format.");
                }

                Guid itemId;
                Guid saleId;

                if (!Guid.TryParse(keys[0], out itemId) || !Guid.TryParse(keys[1], out saleId))
                {
                    return BadRequest("Key has invalid format.");
                }

                var countryId = keys[2];

                var product = Mapper.Map<Product, Models.Product>(_service.GetProduct(itemId, saleId, countryId));

                if (product == null)
                {
                    return NotFound();
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
