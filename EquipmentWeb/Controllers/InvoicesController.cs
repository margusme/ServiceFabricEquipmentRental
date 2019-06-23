using System;
using System.Collections.Generic;
using System.Fabric.Query;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using EquipmentData.Globals;
using System.IO;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Threading;

namespace EquipmentWeb.Controllers
{
    /// <summary>
    /// Invoices controller for downloading invoices
    /// </summary>
    [Produces("application/json")]
    [Route("api/Invoices")]
    public class InvoicesController : BaseController<InvoicesController>
    {
        protected readonly IStringLocalizer<InvoicesController> _localizer;        

        public InvoicesController(IStringLocalizer<InvoicesController> localizer, ICommon common) : base(common)
        {
            _localizer = localizer;            
        }

        /// <summary>
        /// Download last invoice
        /// </summary>
        /// <returns></returns>
        [HttpGet("Last")]
        public async Task<IActionResult> DownloadLast()
        {
            _logger.Log(LogLevel.Information, "User started to download last invoice file");
            return await GetInvoiceFile();
        }

        /// <summary>
        /// Download all invoices
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        public async Task<IActionResult> Download()
        {
            _logger.Log(LogLevel.Information, "User started to download all invoices file");
            return await GetInvoiceFile(false);
        }

        /// <summary>
        /// Download whether last or all invoices as one file
        /// </summary>
        /// <param name="downloadLastInvoice">If true then will download only last basket invoice</param>
        /// <returns></returns>
        protected async Task<FileStreamResult> GetInvoiceFile(bool downloadLastInvoice = true)
        {            
            var result = await GetInvoiceData(downloadLastInvoice);

            byte[] textAsBytes = GetInvoiceBytes(result);

            MemoryStream memory = new MemoryStream(textAsBytes);

            memory.Position = 0;
            _logger.Log(LogLevel.Information, "Invoices file ready");
            return File(memory, "text/plain", "Invoice.txt");
        }

        /// <summary>
        /// Reads invoice data from backend statefil service and gets back list with the data
        /// </summary>
        /// <param name="downloadLastInvoice">If true then will download only last basket invoice</param>
        /// <returns></returns>
        protected async Task<List<KeyValuePair<Guid, Invoice>>> GetInvoiceData(bool downloadLastInvoice = true)
        {
            Uri serviceName = _common.GetEquipmentDataServiceName();

            var partitions = await this._fabricClient.QueryManager.GetPartitionListAsync(serviceName);

            var result = new List<KeyValuePair<Guid, Invoice>>();

            foreach (Partition partition in partitions)
            {
                string proxyUrl = _common.GetInvoicesProxyGetUrl(partition, downloadLastInvoice);

                using (HttpResponseMessage response = await this._httpClient.GetAsync(proxyUrl))
                {
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        continue;
                    }

                    result.AddRange(JsonConvert.DeserializeObject<List<KeyValuePair<Guid, Invoice>>>(await response.Content.ReadAsStringAsync()));
                }
            }

            return result;
        }

        /// <summary>
        /// Reads list with invoices data and return back byte array
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected byte[] GetInvoiceBytes(List<KeyValuePair<Guid, Invoice>> result)
        {
            var culture = CultureInfo.CurrentUICulture;
            var stringBuilder = new StringBuilder(_localizer["RentalInvoices"] + Environment.NewLine + Environment.NewLine);
            long price = 0;
            int bonuses = 0;

            foreach (KeyValuePair<Guid, Invoice> pair in result)
            {
                if (pair.Value.Price == 0)
                {
                    continue;
                }

                stringBuilder.AppendLine(_localizer["Invoice for"] + " " + pair.Value.Title + ", " + _localizer["tracking no"] + ": " + pair.Key + Environment.NewLine);
                foreach (InvoiceRow row in pair.Value.Rows)
                {
                    stringBuilder.AppendLine(row.Name + "\t\t\t" + row.Price + "€");
                }
                stringBuilder.AppendLine(_localizer["Invoice total"]);
                stringBuilder.AppendLine(_localizer["Price"] + ": " + pair.Value.Price + "€\t\t\t" + (_localizer["Earned bonuses"] + ": " + pair.Value.Bonuses + Environment.NewLine));
                price += (long)pair.Value.Price;
                bonuses += pair.Value.Bonuses;
            }

            stringBuilder.AppendLine(_localizer["All Invoice total"]);
            stringBuilder.AppendLine(_localizer["Price"] + ": " + price + "€\t\t\t" + _localizer["Earned bonuses"] + ": " + bonuses + Environment.NewLine);

            return Encoding.Unicode.GetBytes(stringBuilder.ToString());
        }
    }
}
