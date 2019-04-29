using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using EquipmentData.Globals;
using Microsoft.Extensions.Logging;
using System.IO;

namespace EquipmentData.Controllers
{
    /// <summary>
    /// Controller for holding all available equipment in the system
    /// </summary>
    [Route("api/[controller]")]
    public class EquipmentDataController : BaseController
    {        
        protected const int TextLineItemCount = 3;
        protected const string FileSeparator = ",";
        protected const string EquipmentFileName = @"equipment.txt";

        public EquipmentDataController(ILogger<EquipmentDataController> logger, IReliableStateManager stateManager) : base(logger, stateManager)
        {
            Task.Run(() => this.InitData()).Wait();
        }

        /// <summary>
        /// Reads equipment from file and uploads into dictionary
        /// </summary>
        /// <returns></returns>
        protected async Task<IActionResult> InitData()
        {
            _logger.Log(LogLevel.Information, "Init equipment data from file");
            var nameTypeDictionary = await GetNameTypeDictionary();
            var equipmentCountDictionary = await GetEquipmentCountDictionary();
            var equipmentRentalDictionary = await GetEquipmentRentalDictionary();

            using (ITransaction tx = this._stateManager.CreateTransaction())
            {
                TextReader tr = new StreamReader(EquipmentFileName);

                string line = "";
                while ((line = await tr.ReadLineAsync()) != null)
                {
                    await AddNewEquipmentFromFile(tx, nameTypeDictionary, equipmentCountDictionary, equipmentRentalDictionary, line);
                }

                await tx.CommitAsync();
            }

            _logger.Log(LogLevel.Information, "Finished init equipment data from file");

            return new OkResult();
        }

        /// <summary>
        /// Gets all equipment data and shows inside json to the consumer service
        /// 
        /// GET api/equipmentData
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            _logger.Log(LogLevel.Information, "Start reading all equipment data from dictionary");
            CancellationToken ct = new CancellationToken();

            var nameTypeDictionary = await GetNameTypeDictionary();
            var equipmentCountDictionary = await GetEquipmentCountDictionary();

            using (ITransaction tx = this._stateManager.CreateTransaction())
            {
                var list = await nameTypeDictionary.CreateEnumerableAsync(tx);
                var enumerator = list.GetAsyncEnumerator();
                var result = new List<KeyValuePair<string, EquipmentType>>();

                while (await enumerator.MoveNextAsync(ct))
                {
                    var equipmentName = enumerator.Current.Key;
                    
                    var initialCountResult = await equipmentCountDictionary.TryGetValueAsync(tx, enumerator.Current.Key);
                    if (initialCountResult.HasValue)
                    {
                        var elementRentalDictionary = await GetEquipmentElementRentalDictionary(tx, equipmentName);
                        var elementCount = elementRentalDictionary.Count;

                        if (initialCountResult.Value - elementCount > 0)
                        {                            
                            result.Add(enumerator.Current);
                        }
                        else
                        {
                            result.Add(new KeyValuePair<string, EquipmentType>(enumerator.Current.Key + " (Out of stock)", enumerator.Current.Value));
                        }
                    }                   
                }

                _logger.Log(LogLevel.Information, "Finished reading all equipment data from dictionary");

                return this.Json(result);
            }
        }

        protected async Task<IActionResult> AddNewEquipmentFromFile(ITransaction tx, 
                                                                    IReliableDictionary<string, EquipmentType> nameTypeDictionary, 
                                                                    IReliableDictionary<string, int> equipmentCountDictionary,
                                                                    IReliableDictionary2<string, Uri> equipmentRentalDictionary,
                                                                    string line)
        {
            var equipmentData = line.Split(FileSeparator);
            if (equipmentData.Length == TextLineItemCount)
            {
                var equipmentName = equipmentData[0].Trim();
                if (equipmentName.Length > 0)
                {
                    var equipmentType = (EquipmentType)Enum.Parse(typeof(EquipmentType), equipmentData[1].Trim());
                    int count = GetCount(equipmentData[2]);

                    if (count > 0)
                    {
                        await nameTypeDictionary.SetAsync(tx, equipmentName, equipmentType);
                        await equipmentCountDictionary.SetAsync(tx, equipmentName, count);
                        var uri = new Uri(GetRentalElementDictionaryName(equipmentName));
                        await equipmentRentalDictionary.SetAsync(tx, equipmentName, uri);
                    }
                }
            }

            return new OkResult();
        }

        /// <summary>
        /// Gets count of available equipment item from string
        /// </summary>
        /// <param name="countString"></param>
        /// <returns></returns>
        protected int GetCount(string countString)
        {
            int count = 1;
            try
            {
                count = Int32.Parse(countString.Trim());
            }
            catch (Exception)
            {
                count = 0;
            }

            return count;
        }

    }
}
