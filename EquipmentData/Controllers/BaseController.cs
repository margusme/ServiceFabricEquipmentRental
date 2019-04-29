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

namespace EquipmentData.Controllers
{
    /// <summary>
    /// Base controller
    /// </summary>
    public class BaseController : Controller
    {
        protected readonly IReliableStateManager _stateManager;
        protected ILogger _logger;

        protected static readonly Uri nameTypeDictionaryUri = new Uri(Locals.Constants.EquipmentDataDictionaryName);
        protected static readonly Uri equipmentCountUri = new Uri(Locals.Constants.EquipmentCountDictionaryName);
        protected static readonly Uri equipmentRentalDictionaryUri = new Uri(Locals.Constants.EquipmentRentalDictionaryName);
        protected static readonly Uri equipmentOrderDictionaryUri = new Uri(Locals.Constants.EquipmentOrderDictionaryName);
        protected static readonly Uri basketDictionaryUri = new Uri(Locals.Constants.BasketDictionaryName);
        protected static readonly Uri equipmentOrderQueueUri = new Uri(Locals.Constants.EquipmentOrderQueueName);

        protected readonly List<Uri> rentalQueues = new List<Uri>();

        public BaseController(ILogger logger, IReliableStateManager stateManager)
        {
            _logger = logger;
            this._stateManager = stateManager;
        }

        /// <summary>
        /// Gets equipment name to type relations dictionary
        /// </summary>
        /// <returns></returns>
        protected async Task<IReliableDictionary<string, EquipmentType>> GetNameTypeDictionary()
        {
            return await this._stateManager.GetOrAddAsync<IReliableDictionary<string, EquipmentType>>(nameTypeDictionaryUri);
        }

        /// <summary>
        /// Gets equipment initial counts collection for each equipment name
        /// </summary>
        /// <returns></returns>
        protected async Task<IReliableDictionary<string, int>> GetEquipmentCountDictionary()
        {
            return await this._stateManager.GetOrAddAsync<IReliableDictionary<string, int>>(equipmentCountUri);
        }

        /// <summary>
        /// Gets one equipment elements rentals collection.
        /// Can be used in already made transaction
        /// </summary>
        /// <param name="equipmentName"></param>
        /// <returns></returns>
        protected async Task<IReliableDictionary2<Guid, DateTime>> GetEquipmentElementRentalDictionary(ITransaction tx, string equipmentName)
        {
            var rentalDictionary = await GetEquipmentRentalDictionary();
            Uri uri;

            var result = await rentalDictionary.TryGetValueAsync(tx, equipmentName);

            if (result.HasValue)
            {
                uri = result.Value;
            }
            else
            {
                uri = new Uri(GetRentalElementDictionaryName(equipmentName));
                await rentalDictionary.SetAsync(tx, equipmentName, uri);

                await tx.CommitAsync();
            }

            return await this._stateManager.GetOrAddAsync<IReliableDictionary2<Guid, DateTime>>(uri);
        }

        /// <summary>
        /// Gets dictionary for storing dictionary of equipment rentals per each equipment
        /// </summary>
        /// <returns></returns>
        protected async Task<IReliableDictionary2<string, Uri>> GetEquipmentRentalDictionary()
        {
            return await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, Uri>>(equipmentRentalDictionaryUri);
        }

        /// <summary>
        /// Gets dictionary for storing orders in basket
        /// </summary>
        /// <returns></returns>
        protected async Task<IReliableDictionary<Guid, EquipmentOrder>> GetBasketDictionary()
        {
            return await this._stateManager.GetOrAddAsync<IReliableDictionary<Guid, EquipmentOrder>>(basketDictionaryUri);
        }

        /// <summary>
        /// Gets allequipment orders dictionary
        /// </summary>
        /// <returns></returns>
        protected async Task<IReliableDictionary2<Guid, BasketOrderList>> GetEquipmentOrderDictionary()
        {
            return await this._stateManager.GetOrAddAsync<IReliableDictionary2<Guid, BasketOrderList>>(equipmentOrderDictionaryUri);
        }

        /// <summary>
        /// Gets queue for storing last order for invoice
        /// </summary>
        /// <returns></returns>
        protected async Task<IReliableQueue<Guid>> GetEquipmentOrderQueue()
        {
            return await this._stateManager.GetOrAddAsync<IReliableQueue<Guid>>(equipmentOrderQueueUri);
        }

        protected async Task<Guid> GetLastOrdersQueueNumber()
        {
            var queue = await GetEquipmentOrderQueue();

            using (ITransaction tx = this._stateManager.CreateTransaction())
            {
                Guid value;

                ConditionalValue<Guid> result = await queue.TryPeekAsync(tx);

                if (!result.HasValue)
                {
                    //First time reading from queue
                    value = Guid.NewGuid();
                    await queue.EnqueueAsync(tx, value);
                }
                else
                {
                    value = result.Value;
                }

                await tx.CommitAsync();

                return value;
            }
        }

        protected async Task<Guid> EnqueueNewOrdersNumber()
        {
            var queue = await GetEquipmentOrderQueue();

            using (ITransaction tx = this._stateManager.CreateTransaction())
            {
                Guid value = Guid.NewGuid();

                await queue.TryDequeueAsync(tx);
                await queue.EnqueueAsync(tx, value);

                await tx.CommitAsync();

                return value;
            }
        }

        protected string GetRentalElementDictionaryName(string equipmentName)
        {
            return Locals.Constants.EquipmentRentalDictionaryName + "/" + equipmentName.Replace(" ", "_");
        }
    }
}
