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
    /// Controller deals with equipment orders/rentals in the basket
    /// </summary>
    [Route("api/[controller]")]
    public class BasketDataController : BaseController
    {
        public BasketDataController(ILogger<BasketDataController> logger, IReliableStateManager stateManager) : base(logger, stateManager)
        {

        }

        /// <summary>
        /// Gets all added basket items
        /// 
        /// GET api/basketData
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            _logger.Log(LogLevel.Information, "Start reading all added basket items");

            CancellationToken ct = new CancellationToken();
            var basketDictionary = await GetBasketDictionary();

            using (ITransaction tx = this._stateManager.CreateTransaction())
            {
                var list = await basketDictionary.CreateEnumerableAsync(tx);

                var enumerator = list.GetAsyncEnumerator();

                var result = new List<KeyValuePair<Guid, EquipmentOrder>>();

                while (await enumerator.MoveNextAsync(ct))
                {
                    result.Add(enumerator.Current);
                }

                _logger.Log(LogLevel.Information, "Finished reading all added basket items");

                return this.Json(result);
            }
        }

        /// POST api/basketData/close
        /// <summary>
        /// Method closes basket and moves all items into order dictionary
        /// </summary>
        /// <returns></returns>
        [HttpPost("close")]
        public async Task<IActionResult> Close()
        {
            _logger.Log(LogLevel.Information, "Start closing basket");
            CancellationToken ct = new CancellationToken();
            var guid = await EnqueueNewOrdersNumber();
            var basketDictionary = await GetBasketDictionary();            
            var equipmentOrderDictionary = await GetEquipmentOrderDictionary();

            using (ITransaction tx = this._stateManager.CreateTransaction())
            {
                var basketList = new BasketOrderList();
                var list = await basketDictionary.CreateEnumerableAsync(tx);
                var enumerator = list.GetAsyncEnumerator();                

                while (await enumerator.MoveNextAsync(ct))
                {                    
                    basketList.Add(enumerator.Current.Value);
                }
                enumerator = list.GetAsyncEnumerator();

                await equipmentOrderDictionary.AddAsync(tx, guid, basketList);
                //await basketDictionary.ClearAsync();
                await tx.CommitAsync();
            }

            await ClearBasket();

            _logger.Log(LogLevel.Information, "Finished closing basket");

            return new OkResult();
        }

        /// <summary>
        /// Puts new item into the basket for days specified
        /// 
        /// PUT api/basketData/name
        /// </summary>
        /// <param name="name">Item name</param>
        /// <param name="days">Item days for renting</param>
        /// <returns></returns>
        [HttpPut("{name}/{days}")]      
        public async Task<IActionResult> Put(string name, int days = Globals.Constants.MinimumOrderDays)
        {
            _logger.Log(LogLevel.Information, $"Start adding to basket {name}/{days}");
            days = days < 1 ? 1 : days;
            days = days > Globals.Constants.MaximumOrderDays ? Globals.Constants.MaximumOrderDays : days;

            var guid = Guid.NewGuid();
            var type = await GetEquipmentType(name);
            var order = new EquipmentOrder() { Days = days, Name = name, OrderTime = DateTime.Now, Type = type };
            
            var basketDictionary = await GetBasketDictionary();
            var equipmentCountDictionary = await GetEquipmentCountDictionary();

            var keys = await GetEquipmentRentals(name);

            await RemoveExpiredRentals(keys, name);
            await AddToBasket(equipmentCountDictionary, basketDictionary, name, guid, order);

            _logger.Log(LogLevel.Information, $"Finished adding to basket {name}/{days}");

            return new OkResult();
        }

        /// <summary>
        /// Deletes item from basket
        /// 
        ///  DELETE api/basketData/guid
        /// </summary>
        /// <param name="guid">Rental guid in the basket</param>
        /// <returns></returns>
        [HttpDelete("{guid}")]
        public async Task<IActionResult> Delete(Guid guid)
        {
            _logger.Log(LogLevel.Information, $"Start deleting from basket {guid}");
            var basketDictionary = await GetBasketDictionary();

            using (ITransaction tx = this._stateManager.CreateTransaction())
            {                
                if (await basketDictionary.ContainsKeyAsync(tx, guid))
                {
                    var order = await basketDictionary.TryGetValueAsync(tx, guid);

                    if (order.HasValue)
                    {
                        var elementRentalDictionary = await GetEquipmentElementRentalDictionary(tx, order.Value.Name);
                        await elementRentalDictionary.TryRemoveAsync(tx, guid);
                    }
                    
                    await basketDictionary.TryRemoveAsync(tx, guid);
                    await tx.CommitAsync();

                    _logger.Log(LogLevel.Information, $"Finished deleting from basket {guid}");
                    return new OkResult();
                }
                else
                {
                    _logger.Log(LogLevel.Information, $"Finished deleting from basket, {guid} not found");
                    return new NotFoundResult();
                }
            }
        }

        /// <summary>
        /// Clears basket
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> ClearBasket()
        {
            _logger.Log(LogLevel.Information, $"Start clearing basket");
            var basketDictionary = await GetBasketDictionary();

            CancellationToken ct = new CancellationToken();
            var keys = new List<Guid>();

            using (ITransaction tx = this._stateManager.CreateTransaction())
            {
                var list = await basketDictionary.CreateEnumerableAsync(tx);
                var enumerator = list.GetAsyncEnumerator();

                while (await enumerator.MoveNextAsync(ct))
                {
                    keys.Add(enumerator.Current.Key);
                }

                await tx.CommitAsync();
            }

            await RemoveBasketKeys(basketDictionary, keys);

            _logger.Log(LogLevel.Information, $"Finished clearing basket");

            return new OkResult();
        }

        /// <summary>
        /// Gets equipment type according to the name
        /// </summary>
        /// <param name="name">Equipment name</param>
        /// <returns></returns>
        protected async Task<EquipmentType> GetEquipmentType(string name)
        {
            var nameTypeDictionary = await GetNameTypeDictionary();

            using (ITransaction tx = this._stateManager.CreateTransaction())
            {
                EquipmentType type;
                var result = await nameTypeDictionary.TryGetValueAsync(tx, name);

                if (result.HasValue)
                {
                    type = result.Value;
                }
                else
                {
                    throw new Exception("Equipment type not found");
                }

                await tx.CommitAsync();

                return type;
            }
        }

        /// <summary>
        /// Removes data from the basket by prepared keys
        /// </summary>
        /// <param name="basketDictionary">Dictionary that is to be cleared</param>
        /// <param name="keys">Guids collection</param>
        /// <returns></returns>
        protected async Task<IActionResult> RemoveBasketKeys(IReliableDictionary<Guid, EquipmentOrder> basketDictionary, List<Guid> keys)
        {
            foreach (Guid key in keys)
            {
                /// https://stackoverflow.com/questions/41370958/automatically-expire-service-fabric-reliable-dictionary-objects-via-runasync

                using (ITransaction tx = this._stateManager.CreateTransaction())
                {
                    if (await basketDictionary.ContainsKeyAsync(tx, key))
                    {
                        await basketDictionary.TryRemoveAsync(tx, key);
                        await tx.CommitAsync();
                    }
                }
            }

            return new OkResult();
        }

        /// <summary>
        /// Gets all rental guids related to the equipment
        /// </summary>
        /// <param name="name">Equipment name</param>
        /// <returns></returns>
        protected async Task<List<Guid>> GetEquipmentRentals(string name)
        {
            CancellationToken ct = new CancellationToken();
            var keys = new List<Guid>();

            using (ITransaction tx = this._stateManager.CreateTransaction())
            {
                var elementRentalDictionary = await GetEquipmentElementRentalDictionary(tx, name);

                var list = await elementRentalDictionary.CreateEnumerableAsync(tx);
                var enumerator = list.GetAsyncEnumerator();

                while (await enumerator.MoveNextAsync(ct))
                {
                    keys.Add(enumerator.Current.Key);
                }
            }

            return keys;
        }

        /// <summary>
        /// Removes equipment expired rentals
        /// </summary>
        /// <param name="keys">Guids for the expured rentals</param>
        /// <param name="name">Equipment name</param>
        /// <returns></returns>
        protected async Task<IActionResult> RemoveExpiredRentals(List<Guid> keys, string name)
        {
            foreach (Guid key in keys)
            {
                /// https://stackoverflow.com/questions/41370958/automatically-expire-service-fabric-reliable-dictionary-objects-via-runasync

                using (ITransaction tx = this._stateManager.CreateTransaction())
                {
                    var elementRentalDictionary = await GetEquipmentElementRentalDictionary(tx, name);
                    var result = await elementRentalDictionary.TryGetValueAsync(tx, key);

                    if (result.HasValue && result.Value < DateTime.Now)
                    {
                        await elementRentalDictionary.TryRemoveAsync(tx, key);
                        await tx.CommitAsync();
                    }
                }
            }

            return new OkResult();
        }

        /// <summary>
        /// Adds data to the basket and equipment rentals dictionary
        /// </summary>
        /// <param name="equipmentCountDictionary">Initial count dictionary for equipment</param>
        /// <param name="basketDictionary">Basket dictionary</param>
        /// <param name="name">Equipment name</param>
        /// <param name="guid">Rental guid</param>
        /// <param name="order">Order of the rental</param>
        /// <returns></returns>
        protected async Task<IActionResult> AddToBasket(IReliableDictionary<string, int> equipmentCountDictionary, IReliableDictionary<Guid, EquipmentOrder> basketDictionary, string name, Guid guid, EquipmentOrder order)
        {
            using (ITransaction tx = this._stateManager.CreateTransaction())
            {
                var initialCountResult = await equipmentCountDictionary.TryGetValueAsync(tx, name);

                if (initialCountResult.HasValue)
                {
                    var elementRentalDictionary = await GetEquipmentElementRentalDictionary(tx, name);
                    var elementCount = elementRentalDictionary.Count;

                    if (initialCountResult.Value - elementCount > 0)
                    {
                        await basketDictionary.AddAsync(tx, guid, order);

                        await elementRentalDictionary.AddAsync(tx, guid, order.OrderTime.AddDays(order.Days));
                        await tx.CommitAsync();
                    }
                }
            }

            return new OkResult();
        }
    }
}
