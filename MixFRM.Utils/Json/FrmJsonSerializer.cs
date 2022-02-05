using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MixFRM.Utils.Json
{
    public static class FrmJsonSerializer
    {
        static FrmJsonSerializer()
        {

        }

        public static string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value);
        }

        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
