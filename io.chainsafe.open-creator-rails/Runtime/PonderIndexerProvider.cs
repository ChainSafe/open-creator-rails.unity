using System;
using System.Linq;
using System.Numerics;
using System.Text;
using Cysharp.Threading.Tasks;
using Io.ChainSafe.OpenCreatorRails.Contracts.Asset.Service;
using Io.ChainSafe.OpenCreatorRails.Contracts.AssetRegistry.Service;
using Io.ChainSafe.OpenCreatorRails.DTOs;
using Io.ChainSafe.OpenCreatorRails.Utils;
using Nethereum.Hex.HexConvertors.Extensions;
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

        public async UniTask<AssetRegistry> GetAssetRegistry(EthereumAddress registryAddress)
        {
            // TODO: this should be replaced once AssetRegistry entity is implemented on the Indexer
            AssetRegistryService service =
                new AssetRegistryService(OpenCreatorRailsService.Instance.Web3, registryAddress.Value);

            BigInteger registryFeeShare = await service.GetRegistryFeeShareQueryAsync();

            EthereumAddress owner = new EthereumAddress(await service.GetOwnerQueryAsync());

            return new AssetRegistry(registryAddress, registryFeeShare, owner);
        }

        public async UniTask<Asset[]> GetAssets(EthereumAddress registryAddress)
        {
            string query = $@"
{{
    entity: assetEntitys(where: {{ registryAddress: ""{registryAddress}"" }}) {{
        items {{
            assetId
            address
            owner
        }}
    }}
    
    creation: assetRegistry_AssetCreateds(where: {{ registryAddress: ""{registryAddress}"" }}) {{
        items {{
            assetId
            subscriptionPrice
            tokenAddress
        }}
    }}

    subscriptions
    {{
        items {{
            assetId
            subscriber
            payer
            startTime
            endTime
            nonce
            isActive
        }}
    }}
}}";

            JToken response = await Query<JToken>(query);

            var entities = response["entity"]?["items"]?.Values<JToken>() ?? new JArray();

            var creations = response["creation"]?["items"]?.Values<JToken>() ?? new JArray();

            var subscriptions = response["subscriptions"]?["items"]?.Values<JToken>() ?? new JArray();

            return entities?.Select(entity =>
                {
                    string assetIdHash = entity.Value<JToken>("assetId").Value<string>();

                    EthereumAddress address = new EthereumAddress(entity.Value<string>("address"));
                    EthereumAddress owner = new EthereumAddress(entity.Value<string>("owner"));

                    JToken creation = creations.First(c => c["assetId"].ToObject<string>().Equals(assetIdHash));

                    BigInteger subscriptionPrice = BigInteger.Parse(creation.Value<string>("subscriptionPrice"));
                    EthereumAddress tokenAddress = new EthereumAddress(creation.Value<string>("tokenAddress"));

                    BigInteger chainId = OpenCreatorRailsService.Instance.WalletProvider.ChainId;

                    Subscription[] assetSubscriptions = subscriptions
                        .Where(subscription => subscription.Value<string>("assetId") == $"{(int)chainId}_{address}")
                        .Select(subscription =>
                        {
                            string subscriberIdHash = subscription.Value<string>("subscriber");
                            EthereumAddress payer = new EthereumAddress(subscription.Value<string>("payer"));
                            DateTime startTime = subscription["startTime"].FromUnixLongToDateTime();
                            DateTime endTime = subscription["endTime"].FromUnixLongToDateTime();
                            bool isActive = subscription.Value<bool>("isActive");
                            BigInteger nonce = new BigInteger(subscription.Value<long>("nonce"));

                            return new Subscription(assetIdHash, subscriberIdHash, payer, startTime, endTime, isActive,
                                nonce,
                                registryAddress);
                        }).ToArray();

                    return new Asset(assetIdHash, address, subscriptionPrice, owner, tokenAddress, registryAddress,
                        assetSubscriptions);
                })
                .ToArray();
        }

        public async UniTask<Subscription[]> GetAssetSubscriptions(string assetIdHash, EthereumAddress registryAddress)
        {
            // TODO: indexer should get subscriptions via assetId and registryAddress not `assetId = {chainId}_{assetAddress}`
            JToken response = await Query<JToken>($@"
{{
    assetEntitys(
        where: {{
            assetId: ""{assetIdHash}""
            registryAddress: ""{registryAddress.Value}""
        }}
        limit: 1
      ) {{
            items {{
                address
            }}
        }}
}}");
            string address = response["assetEntitys"]?["items"]?.Values<JToken>().FirstOrDefault()?["address"]
                ?.Value<string>();

            BigInteger chainId = OpenCreatorRailsService.Instance.WalletProvider.ChainId;

            response = await Query<JToken>($@"
{{
    subscriptions(where: {{ assetId: ""{(long)chainId}_{address}"" }})
    {{
        items {{
            subscriber
            payer
            startTime
            endTime
            nonce
            isActive
        }}
    }}
}}");
            return response["subscriptions"]?["items"]
                ?.Values<JToken>()
                .Select(item =>
                {
                    string subscriberIdHash = item.Value<string>("subscriber");
                    EthereumAddress payer = new EthereumAddress(item.Value<string>("payer"));
                    DateTime startTime = item["startTime"].FromUnixLongToDateTime();
                    DateTime endTime = item["endTime"].FromUnixLongToDateTime();
                    bool isActive = item.Value<bool>("isActive");
                    BigInteger nonce = new BigInteger(item.Value<long>("nonce"));

                    return new Subscription(assetIdHash, subscriberIdHash, payer, startTime, endTime, isActive, nonce,
                        registryAddress);
                })
                .ToArray();
        }

        public async UniTask<bool> HasAccess(string subscriberIdHash, string assetIdHash, EthereumAddress registryAddress)
        {
            // TODO: we should fetch this from the indexer and not an RPC call
            JToken response = await Query<JToken>($@"
{{
    assetEntitys(
        where: {{
            assetId: ""{assetIdHash}""
            registryAddress: ""{registryAddress.Value}""
        }}
        limit: 1
      ) {{
            items {{
                address
            }}
        }}
}}");
            
            string address = response["assetEntitys"]?["items"]?.Values<JToken>().FirstOrDefault()?["address"]
                ?.Value<string>();
            
            AssetService assetService = new AssetService(OpenCreatorRailsService.Instance.Web3, address);

            return await assetService.IsSubscriptionActiveQueryAsync(subscriberIdHash.HexToByteArray());
        }
    }
}