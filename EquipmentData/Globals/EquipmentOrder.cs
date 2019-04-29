using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using EquipmentData.Globals;

namespace EquipmentData.Globals
{
    public class EquipmentOrder
    {
        private DateTime orderTime;
        private string name;
        private EquipmentType type;
        private int days;

        public DateTime OrderTime { get => orderTime; set => orderTime = value; }
        public string Name { get => name; set => name = value; }
        public EquipmentType Type { get => type; set => type = value; }
        public int Days { get => days; set => days = value; }
    }
}
