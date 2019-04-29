using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace EquipmentData.Globals
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EquipmentType
    {
        [EnumMember(Value = "Heavy")]
        Heavy,
        [EnumMember(Value = "Regular")]
        Regular,
        [EnumMember(Value = "Specialized")]
        Specialized
    };

    public class Constants
    {
        public const int MinimumOrderDays = 1;
        public const int MaximumOrderDays = 365;
    }
}