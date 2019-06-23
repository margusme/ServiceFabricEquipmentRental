using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Query;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using EquipmentData.Globals;
using Microsoft.Extensions.Logging;

namespace EquipmentWeb.Controllers
{
    /// <summary>
    /// Class for showing equipment related data to the user
    /// </summary>
    [Produces("application/json")]
    [Route("api/Equipments")]
    public class EquipmentsController : BaseController<EquipmentsController>
    {
        public EquipmentsController(ICommon common) : base(common)
        {

        }

        /// <summary>
        /// Downloads equipments and shows to the user
        /// 
        /// GET: api/Equipments
        /// </summary>
        /// <returns></returns>        
        [HttpGet("")]
        public async Task<IActionResult> Get()
        {
            _logger.Log(LogLevel.Information, "Read equipment data from backend");
            Uri serviceName = _common.GetEquipmentDataServiceName();

            var partitions = await this._fabricClient.QueryManager.GetPartitionListAsync(serviceName);

            var result = new List<KeyValuePair<string, EquipmentType>>();

            foreach (Partition partition in partitions)
            {
                string proxyUrl = _common.GetEquipmentsProxyGetUrl(partition);

                using (HttpResponseMessage response = await this._httpClient.GetAsync(proxyUrl))
                {
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        continue;
                    }

                    result.AddRange(JsonConvert.DeserializeObject<List<KeyValuePair<string, EquipmentType>>>(await response.Content.ReadAsStringAsync()));
                }
            }

            _logger.Log(LogLevel.Information, "Finished reading equipment data from backend");

            return this.Json(result);
        }
    }
}
