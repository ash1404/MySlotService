using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServiceApiCaller
{
    public class SlotFormat
    {
        public int center_id { get; set; }

        public string name { get; set; }

        public string address { get; set; }

        public string state_name { get; set; }

        public string district_name { get; set; }

        public string block_name { get; set; }

        public int pincode { get; set; }
        public int lat { get; set; }
        [JsonProperty("long")]
        public int _long { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public string fee_type { get; set; }

        public List<DayWiseSlots> DayWiseSlots  { get; set; }

    }
}
