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
    /// <summary>
    /// Built-in <see cref="IIndexerProvider"/> implementation that queries a
    /// <a href="https://ponder.sh">Ponder</a>-based GraphQL indexer over HTTP. Add this component
    /// to the same GameObject as <see cref="OpenCreatorRailsService"/> and set the
    /// <c>Indexer Url</c> field in the Inspector to the base URL of your deployed indexer.
    /// </summary>
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
    assets(
        where: {{
            assetId: ""{assetIdHash}""
            registryAddress: ""{registryAddress}""
        }}
limit: 1)
    {{
        items {{
            address
            subscriptionPrice
            subscriptionDuration
            owner
            tokenAddress
            subscriptions {{
                items {{
                    subscriber
                    nonce
                    startTime
                    endTime
                    subscriptionPrice
                    registryFeeShare
                    payer
                    isExpired
                    isRevoked
                    isActive
                }}
            }}
        }}
    }}
}}
");
             JToken asset = response["assets"]?["items"]?.Values<JToken>().First() ?? throw new InvalidOperationException();
             
             EthereumAddress address = new EthereumAddress(asset.Value<string>("address"));
             BigInteger subscriptionPrice = BigInteger.Parse(asset.Value<string>("subscriptionPrice"));
             TimeSpan subscriptionDuration = TimeSpan.FromSeconds(asset.Value<long>("subscriptionDuration"));
             EthereumAddress owner = new EthereumAddress(asset.Value<string>("owner"));
             EthereumAddress tokenAddress = new EthereumAddress(asset.Value<string>("tokenAddress"));
             
             List<SubscriptionDto> subscriptions = asset?["subscriptions"]?["items"]?.Values<JToken>().Select(subscription =>
             {
                 string subscriberIdHash = subscription.Value<string>("subscriber");
                 BigInteger nonce = BigInteger.Parse(subscription.Value<string>("nonce"));
                 DateTime startTime = DateTimeOffset.FromUnixTimeSeconds(subscription.Value<long>("startTime")).DateTime;
                 DateTime endTime = DateTimeOffset.FromUnixTimeSeconds(subscription.Value<long>("endTime")).DateTime;
                 BigInteger registryFeeShare = BigInteger.Parse(subscription.Value<string>("registryFeeShare"));
                 BigInteger subscribedAtPrice = BigInteger.Parse(subscription.Value<string>("subscriptionPrice"));
                 EthereumAddress payer = new EthereumAddress(subscription.Value<string>("payer"));
                 bool isExpired = subscription.Value<bool>("isExpired");
                 bool isRevoked = subscription.Value<bool>("isRevoked");
                 bool isActive = subscription.Value<bool>("isActive");
                 
                 return new SubscriptionDto(subscriberIdHash, nonce, startTime, endTime, subscribedAtPrice, registryFeeShare, payer, isExpired, isRevoked, isActive);
             }).ToList();

             return new AssetDto(address, subscriptionPrice, subscriptionDuration, owner, tokenAddress, subscriptions);
        }
    }
}