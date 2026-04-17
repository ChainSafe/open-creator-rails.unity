using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Io.ChainSafe.OpenCreatorRails.Utils
{
    public static class Secrets
    {
        public static readonly string FilePath = Path.Combine(Application.dataPath, "secrets.json");
        
        public static T Get<T>(string key)
        {
            if (!File.Exists(FilePath))
            {
                throw new FileNotFoundException("File not found", FilePath);
            }

            string text = File.ReadAllText(FilePath);

            JObject json = JObject.Parse(text);

            if (!json.TryGetValue(key, out JToken value))
            {
                throw new KeyNotFoundException($"\"{key}\" Key not found");
            }

            return value.ToObject<T>();
        }
    }
}