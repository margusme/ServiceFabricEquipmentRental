using System.Fabric;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EquipmentWeb.Controllers
{
    /// <summary>
    /// Base controller
    /// </summary>
    public class BaseController<T> : Controller where T : BaseController<T>
    {
        protected readonly HttpClient _httpClient;
        protected readonly FabricClient _fabricClient;
        protected ILogger<T> __logger;
        protected readonly ICommon _common;

        protected ILogger<T> _logger => __logger ?? (__logger = HttpContext?.RequestServices.GetService<ILogger<T>>());

        public BaseController(ICommon common)
        {
            _common = common;
            _fabricClient = _common.GetFabricClient();
            _httpClient = _common.GetHttpClient();
        }
    }
}
