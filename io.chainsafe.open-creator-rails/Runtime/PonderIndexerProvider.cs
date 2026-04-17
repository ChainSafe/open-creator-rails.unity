using System;
using System.Linq;
using System.Numerics;
using System.Text;
using Cysharp.Threading.Tasks;
using Io.ChainSafe.OpenCreatorRails.Contracts.AssetRegistry.Service;
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
    entity: assetEntitys(where: {{ registryAddress: ""0xe7f1725e7734ce288f8367e1bb143e90bb3f0512"" }}) {{
        items {{
            assetId
            address
            owner
        }}
    }}
    
    creation: assetRegistry_AssetCreateds(where: {{ registryAddress: ""0xe7f1725e7734ce288f8367e1bb143e90bb3f0512"" }}) {{
        items {{
            assetId
            subscriptionPrice
            tokenAddress
        }}
    }}
}}";

            JToken response = await Query<JToken>(query);

            var entities = response["entity"]?["items"]?.Values<JToken>() ?? new JArray();

            var creations = response["creation"]?["items"]?.Values<JToken>() ?? new JArray();

            return entities?.Select(entity =>
                {
                    string id = entity.Value<JToken>("assetId").Value<string>();

                    EthereumAddress address = new EthereumAddress(entity.Value<string>("address"));
                    EthereumAddress owner = new EthereumAddress(entity.Value<string>("owner"));

                    JToken creation = creations.First(c => c["assetId"].ToObject<string>().Equals(id));

                    BigInteger subscriptionPrice = BigInteger.Parse(creation.Value<string>("subscriptionPrice"));
                    EthereumAddress tokenAddress = new EthereumAddress(creation.Value<string>("tokenAddress"));

                    return new Asset(id, address, subscriptionPrice, owner, tokenAddress, registryAddress);
                })
                .ToArray();
        }

        public UniTask<Subscription> GetSubscription(string assetId, string subscriberId, EthereumAddress registryAddress)
        {
            throw new NotImplementedException();
        }
    }
}