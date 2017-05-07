using System;
using System.Collections.Generic;
using System.IO;
using BundtBot.Extensions;
using BundtCommon.Extensions;
using Newtonsoft.Json;

namespace BundtBot
{
    public static class BundtFig
    {
        const string configFileName = "config.json";
        const string configGlobalFileName = "config-global.json";

        public static string GetValue(string key)
        {
            var configDictionary = ReadConfigAsDictionary(configFileName);
            if (configDictionary.ContainsKey(key)) return configDictionary[key];

            configDictionary = ReadConfigAsDictionary(configGlobalFileName);
            if (configDictionary.DoesNotContainKey(key)) throw new Exception(configGlobalFileName + " is corrupt");
            return configDictionary[key];
        }

        static Dictionary<string, string> ReadConfigAsDictionary(string fileName)
        {
            var configString = File.ReadAllText(fileName);
            return configString.Deserialize<Dictionary<string, string>>();
        }
    }
}