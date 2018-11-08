using System.Reflection;
using System.Web.Http;
using ApacKernel.AspNet.WebApi.Attributes;

namespace IntegrationService.API.Areas.Global.Controllers
{
    [HttpsRequired]
    public class VersionController : ApiController
    {
        [Route("version")]
        [HttpGet]
        public IHttpActionResult Version()
        {
            return Ok(Assembly.GetExecutingAssembly().GetName().Version);
        }
    }
}
