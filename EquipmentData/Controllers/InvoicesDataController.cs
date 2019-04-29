using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using EquipmentData.Globals;
using EquipmentData.Locals;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace EquipmentData.Controllers
{
    /// <summary>
    /// Controller for reading customer invoices
    /// </summary>
    [Route("api/[controller]")]
    public class InvoicesDataController : BaseController
    {
        public InvoicesDataController(ILogger<InvoicesDataController> logger, IReliableStateManager stateManager) : base(logger, stateManager)
        {

        }

        /// <summary>
        /// GET api/InvoicesData
        /// 
        /// Gets all invoices in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            _logger.Log(LogLevel.Information, "Start reading all invoices in the system");
            CancellationToken ct = new CancellationToken();
            var equipmentOrderDictionary = await GetEquipmentOrderDictionary();

            var result = new List<KeyValuePair<Guid, Invoice>>();

            using (ITransaction tx = this._stateManager.CreateTransaction())
            {
                var list = await equipmentOrderDictionary.CreateEnumerableAsync(tx);
                var enumerator = list.GetAsyncEnumerator();

                while (await enumerator.MoveNextAsync(ct))
                {
                    var invoice = PriceCalculator.GetInvoice(enumerator.Current.Value);
                    if (invoice.Rows.Count > 0)
                    {
                        result.Add(new KeyValuePair<Guid, Invoice>(enumerator.Current.Key, invoice));
                    }                    
                }
            }

            _logger.Log(LogLevel.Information, "Finished reading all invoices in the system");

            //List should be relatively small for performance issuea
            return this.Json(result.OrderBy(o => o.Value.Title).ToList());
        }

        /// <summary>
        /// GET api/InvoicesData/Last
        /// 
        /// Gets last invoice
        /// </summary>
        /// <returns></returns>
        [HttpGet("Last")]
        public async Task<IActionResult> GetLastInvoice()
        {
            _logger.Log(LogLevel.Information, "Start reading last invoices in the system");

            var equipmentOrderDictionary = await GetEquipmentOrderDictionary();

            var result = new List<KeyValuePair<Guid, Invoice>>();

            if (equipmentOrderDictionary.Count > 0)
            {
                var lastGuid = await GetLastOrdersQueueNumber();
                using (ITransaction tx = this._stateManager.CreateTransaction())
                {
                    if (await equipmentOrderDictionary.ContainsKeyAsync(tx, lastGuid))
                    {
                        var basketList = await equipmentOrderDictionary.TryGetValueAsync(tx, lastGuid);
                        if (basketList.HasValue)
                        {
                            result.Add(new KeyValuePair<Guid, Invoice>(lastGuid, PriceCalculator.GetInvoice(basketList.Value)));
                        }
                    }
                    else
                    {
                        //Guid not found, it should not happen
                        _logger.Log(LogLevel.Error, $"Reading last invoices in the system, guid {lastGuid} was not in equipmentOrderDictionary");
                    }
                    
                }
            }

            _logger.Log(LogLevel.Information, "Finished reading last invoice in the system");

            return this.Json(result);
        }
    }
}
