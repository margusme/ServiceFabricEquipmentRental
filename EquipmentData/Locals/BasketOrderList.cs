using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using EquipmentData.Globals;

namespace EquipmentData.Locals
{
    /// <summary>
    /// This class is meant to be used for only inserting once a list of one basket orders and reading from it later.
    /// </summary>
    public class BasketOrderList
    {
        private List<EquipmentOrder> orders;

        public BasketOrderList()
        {
            Orders = new List<EquipmentOrder>();
        }

        public List<EquipmentOrder> Orders { get => orders; set => orders = value; }

        public void Add(EquipmentOrder newOrder)
        {
            Orders.Add(newOrder);
        }
    }
}
