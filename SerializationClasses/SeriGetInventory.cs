using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dota2SteamInvCheck.SerializationClasses
{
    public class SeriGetInventory
    {
    }

    public class RootObject
    {
        public bool success { get; set; }
        public Dictionary<String, InventoryEntry> rgInventory { get; set; }
        public List<object> rgCurrency { get; set; }
        public Dictionary<String, DescriptionsEntry> rgDescriptions { get; set; }
    }

    public class DescriptionsEntry
    {
        public int appid { get; set; }
        public String classid { get; set; }
        public String instanceid { get; set; }
        public String name { get; set; }
        public String market_hash_name { get; set; }
        public String market_name { get; set; }
        public String type { get; set; }
        public bool tradable { get; set; }
        public bool marketable { get; set; }
        public int commodity { get; set; }
    }

    public class InventoryEntry
    {
        public string id { get; set; }
        public string classid { get; set; }
        public string instanceid { get; set; }
        public string amount { get; set; }
        public int pos { get; set; }
    }

    public class RootPriceRequest
    {
        public bool success { get; set; }
        public string lowest_price { get; set; }
        public string volume { get; set; }
        public string median_price { get; set; }
    }
}
