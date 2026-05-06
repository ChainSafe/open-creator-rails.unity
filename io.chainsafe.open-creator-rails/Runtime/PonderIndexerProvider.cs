using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Cysharp.Threading.Tasks;
using Io.ChainSafe.OpenCreatorRails.DTOs;
using Io.ChainSafe.OpenCreatorRails.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Io.ChainSafe.OpenCreatorRails
{
    public class PonderIndexerProvider : MonoBehaviour, IIndexerProvider
    {
        [field: SerializeField] public string IndexerUrl { get; private set; }

        public async UniTask<T> Query<T>(string query)
        {
            UnityWebRequest request = new UnityWebRequest(IndexerUrl, UnityWebRequest.kHttpVerbPOST);

            request.SetRequestHeader("Content-Type", "application/json");

            request.SetRequestHeader("Accept", "application/json");

            JObject json = new JObject(new JProperty("query", query));

            byte[] body = Encoding.UTF8.GetBytes(json.ToString());

            request.uploadHandler = new UploadHandlerRaw(body);

            request.downloadHandler = new DownloadHandlerBuffer();

            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new UnityWebRequestException(request);
            }

            string response = request.downloadHandler.text;

            JObject result = JObject.Parse(response);

            JToken data = result["data"];

            return data != null ? data.ToObject<T>() : default;
        }

        public async UniTask<AssetDto> GetAsset(string assetIdHash, EthereumAddress registryAddress)
        {
            JToken response = await Query<JToken>($@"
{{
    assetEntitys(
        where: {{
            assetId: ""{assetIdHash}""
            registryAddress: ""{registryAddress}""
        }}
limit: 1)
    {{
        items {{
            address
            subscriptionPrice
            owner
            tokenAddress
            subscriptions {{
                items {{
                    subscriber
                    payer
                    startTime
                    endTime
                    isActive
                    nonce
                }}
            }}
        }}
    }}
}}
");
             JToken asset = response["assetEntitys"]?["items"]?.Values<JToken>().First() ?? throw new InvalidOperationException();
             
             EthereumAddress address = new EthereumAddress(asset.Value<string>("address"));
             BigInteger subscriptionPrice = BigInteger.Parse(asset.Value<string>("subscriptionPrice"));
             EthereumAddress owner = new EthereumAddress(asset.Value<string>("owner"));
             EthereumAddress tokenAddress = new EthereumAddress(asset.Value<string>("tokenAddress"));
             
             List<SubscriptionDto> subscriptions = asset?["subscriptions"]?["items"]?.Values<JToken>().Select(subscription =>
             {
                 string subscriberIdHash = subscription.Value<string>("subscriber");
                 EthereumAddress payer = new EthereumAddress(subscription.Value<string>("payer"));
                 DateTime startTime = DateTimeOffset.FromUnixTimeSeconds(subscription.Value<long>("startTime")).DateTime;
                 DateTime endTime = DateTimeOffset.FromUnixTimeSeconds(subscription.Value<long>("endTime")).DateTime;
                 bool isActive = subscription.Value<bool>("isActive");
                 BigInteger nonce = BigInteger.Parse(subscription.Value<string>("nonce"));
                 
                 return new SubscriptionDto(subscriberIdHash, payer, startTime, endTime, isActive, nonce);
             }).ToList();

             return new AssetDto(address, subscriptionPrice, owner, tokenAddress, subscriptions);
        }
    }
}