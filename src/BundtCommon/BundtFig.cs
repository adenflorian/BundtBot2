using System.Collections.Generic;
using System.IO;
using BundtBot.Extensions;
using BundtCommon.Extensions;
using Newtonsoft.Json;

namespace BundtBot
{
    public static class BundtFig
    {
        public static string GetValue(string key)
        {
            var configString = File.ReadAllText("config.json");
            var configDictionary = configString.Deserialize<Dictionary<string, string>>();
            return configDictionary[key];
        }
    }
}