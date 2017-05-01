using System.Collections.Generic;
using System.IO;
using BundtCommon.Extensions;
using Newtonsoft.Json;

namespace BundtBot
{
    public static class BundtFig
    {
        public static string GetValue(string key)
        {
            var configString = File.ReadAllText("config.json");
            var configDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(configString);
            return configDictionary[key];
        }
    }
}