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
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace EquipmentWeb.Controllers
{
    /// <summary>
    /// Class for dealing with data in basket: clear, show, delete
    /// </summary>
    [Produces("application/json")]
    [Route("api/Basket")]
    public class BasketController : BaseController
    {
        public BasketController(ILogger<InvoicesController> logger, ICommon common) : base(logger, common)
        {

        }

        /// <summary>
        /// Reads all basket items
        /// GET: api/Basket
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        public async Task<IActionResult> Get()
        {
            _logger.Log(LogLevel.Information, "Start reading basket data from backend");
            Uri serviceName = _common.GetEquipmentDataServiceName();

            ServicePartitionList partitions = await this._fabricClient.QueryManager.GetPartitionListAsync(serviceName);

            var result = new List<KeyValuePair<Guid, EquipmentOrder>>();

            foreach (Partition partition in partitions)
            {
                string proxyUrl = _common.GetBasketProxyGetUrl(partition);

                using (HttpResponseMessage response = await this._httpClient.GetAsync(proxyUrl))
                {
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        continue;
                    }

                    result.AddRange(JsonConvert.DeserializeObject<List<KeyValuePair<Guid, EquipmentOrder>>>(await response.Content.ReadAsStringAsync()));
                }
            }

            _logger.Log(LogLevel.Information, "Finished reading basket data from backend");

            return this.Json(result);
        }

        /// <summary>
        /// Puts new item to the basket for days count
        /// 
        /// PUT: api/basket/name
        /// </summary>
        /// <param name="name">Item(Equipment) name</param>
        /// <param name="days">Days count</param>
        /// <returns></returns>
        [HttpPut("{name}/{days}")]
        public async Task<IActionResult> Put(string name, int days = Constants.MinimumOrderDays)
        {
            _logger.Log(LogLevel.Information, $"Start sending new basket rental data to backend, {name}/{days}");
            days = days < 1 ? 1 : days;
            string proxyUrl = _common.GetBasketProxyPutUrl(name, days);

            StringContent putContent = new StringContent($"{{ 'name' : '{name}', 'days' : '{days}' }}", Encoding.UTF8, "application/json");
            putContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using (HttpResponseMessage response = await this._httpClient.PutAsync(proxyUrl, putContent))
            {
                _logger.Log(LogLevel.Information, $"Finished sending new basket rental data to backend, {name}/{days}");

                return new ContentResult()
                {
                    StatusCode = (int)response.StatusCode,
                    Content = await response.Content.ReadAsStringAsync()
                };
            }            
        }

        /// <summary>
        /// Deletes equipment item from basket
        /// DELETE: api/Basket/guid
        /// </summary>
        /// <param name="guid">Item guid</param>
        /// <returns></returns>
        [HttpDelete("{guid}")]
        public async Task<IActionResult> Delete(Guid guid)
        {
            _logger.Log(LogLevel.Information, $"Start deleting basket rental data from backend, {guid}");

            string proxyUrl = _common.GetBasketProxyDeleteUrl(guid);

            using (HttpResponseMessage response = await this._httpClient.DeleteAsync(proxyUrl))
            {
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return this.StatusCode((int)response.StatusCode);
                }
            }

            _logger.Log(LogLevel.Information, $"Finished deleting basket rental data from backend, {guid}");

            return new OkResult();
        }

        /// <summary>
        /// Closes basket and stores orders permanently for the user
        /// 
        /// POST: api/basket/close
        /// </summary>
        /// <returns></returns>
        [HttpPost("close")]
        public async Task<IActionResult> Close()
        {
            _logger.Log(LogLevel.Information, $"Start closing basket from backend");
            string proxyUrl = _common.GetBasketProxyCloseUrl();

            StringContent putContent = new StringContent($"", Encoding.UTF8, "application/json");
            putContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using (HttpResponseMessage response = await this._httpClient.PostAsync(proxyUrl, new StringContent("")))
            {
                _logger.Log(LogLevel.Information, $"Finished closing basket from backend");
                return new ContentResult()
                {
                    StatusCode = (int)response.StatusCode,
                    Content = await response.Content.ReadAsStringAsync()
                };
            }
        }
    }
}
