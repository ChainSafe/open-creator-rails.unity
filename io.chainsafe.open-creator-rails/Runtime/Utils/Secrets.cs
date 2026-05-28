using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Io.ChainSafe.OpenCreatorRails.Utils
{
    public static class Secrets
    {
        private static readonly string FilePath = Path.Combine(Application.streamingAssetsPath, "secrets.json");
        
        /// <summary>
        /// Reads and deserializes a value by key from the secrets file in <see cref="FilePath"/>.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the value into.</typeparam>
        /// <param name="key">The JSON key to look up (e.g. <c>"mnemonic"</c> or <c>"rpcUrl"</c>).</param>
        /// <returns>The deserialized value of type <typeparamref name="T"/>.</returns>
        /// <exception cref="System.IO.FileNotFoundException">
        /// Thrown if secrets file does not exist at <see cref="FilePath"/>.
        /// </exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">
        /// Thrown if <paramref name="key"/> is not present in the JSON file.
        /// </exception>
        public static async UniTask<T> Get<T>(string key)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            using var request = UnityWebRequest.Get(FilePath);
            
            await request.SendWebRequest();

            string text = request.result == UnityWebRequest.Result.Success
                ? request.downloadHandler.text
                : throw new UnityWebRequestException(request);
#else
            if (!File.Exists(FilePath))
            {
                throw new FileNotFoundException("File not found", FilePath);
            }
            
            string text = await File.ReadAllTextAsync(FilePath);
#endif
            JObject json = JObject.Parse(text);

            if (!json.TryGetValue(key, out JToken value))
            {
                throw new KeyNotFoundException($"\"{key}\" Key not found");
            }

            return value.ToObject<T>();
        }
    }
}