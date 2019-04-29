using System.Fabric;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EquipmentWeb.Controllers
{
    /// <summary>
    /// Base controller
    /// </summary>
    public class BaseController : Controller
    {
        protected readonly HttpClient _httpClient;
        protected readonly FabricClient _fabricClient;
        protected ILogger _logger;
        protected readonly ICommon _common;

        public BaseController(ILogger logger, ICommon common)
        {
            _logger = logger;
            _common = common;
            _fabricClient = _common.GetFabricClient();
            _httpClient = _common.GetHttpClient();
        }
    }
}
