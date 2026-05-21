using System;
using System.Numerics;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Io.ChainSafe.OpenCreatorRails;
using Io.ChainSafe.OpenCreatorRails.Contracts.AssetRegistry.ContractDefinition;
using Io.ChainSafe.OpenCreatorRails.DTOs;
using Io.ChainSafe.OpenCreatorRails.Utils;
using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Util;
using NUnit.Framework;

namespace Tests.Runtime
{
    [TestFixture]
    public class PonderIndexerProviderTests : TestsBase
    {
        // Pre-seeded registry and asset from the Demo scene / seed-local.sh.
        private static readonly EthereumAddress RegistryAddress =
            new EthereumAddress("0xe7f1725E7734CE288F8367e1Bb143E90bb3F0512");

        private const string ExistingAssetId = "default_asset_id_0";

        // Pre-deployed TestToken on Anvil (first contract deployed by account 0, nonce 0).
        private const string TokenAddress = "0x5FbDB2315678afecb367f032d93F642f64180aa3";

        // Anvil account 1 — used as asset owner so it differs from the connected wallet (account 0).
        private const string AssetOwnerAddress = "0x70997970C51812dc3A010C7d01b50e0d17dc79C8";

        private IIndexerProvider IndexerProvider => OpenCreatorRailsService.Instance.IndexerProvider;

        // -------------------------------------------------------------------------
        // Test 1 — GetAsset returns a populated DTO for a known, seeded asset
        // -------------------------------------------------------------------------

        [Test]
        public async Task Test_GetAsset_ReturnsNonDefaultDto()
        {
            var dto = await IndexerProvider.GetAsset(ExistingAssetId.Keccack256(), RegistryAddress);

            Assert.IsFalse(string.IsNullOrEmpty(dto.Address.Value));

            Assert.AreEqual(dto.Address.Value, "0xcafac3dd18ac6c6e92c921884f9e4176737c052c");
        }

        // -------------------------------------------------------------------------
        // Test 2 — GetAsset throws for an asset the indexer has no record of
        // -------------------------------------------------------------------------

        [Test]
        public async Task Test_GetAsset_NonexistentAsset_ThrowsException()
        {
            try
            {
                // PonderIndexerProvider.cs:92 throws InvalidOperationException when the
                // GraphQL response contains an empty items array.
                await IndexerProvider.GetAsset("nonexistent_asset_id".Keccack256(), RegistryAddress);
            }
            catch (InvalidOperationException)
            {
                Assert.Pass();
            }

            Assert.Fail();
        }

        // -------------------------------------------------------------------------
        // Test 3 — Create a brand-new asset on-chain, wait for the Ponder indexer to
        //           pick it up, then verify every indexed property matches what was
        //           submitted to the contract.
        // -------------------------------------------------------------------------

        [Test]
        public async Task Test_GetAsset_CreatedAsset_PropertiesMatchOnChain()
        {
            // --- 1. Connect wallet at index 0 (registry owner on the local Anvil node) ---
            await OpenCreatorRailsService.Instance.Connect(0);

            // --- 2. Bind to the pre-deployed AssetRegistry ---
            var registryService = OpenCreatorRailsService.GetAssetRegistry(RegistryAddress);

            // --- 3. Define test asset parameters ---
            // Use a unique string per run so repeated test executions on a persistent
            // Anvil state do not collide with the AssetAlreadyExists revert.
            string assetIdString = "test_indexer_asset_" + Guid.NewGuid().ToString("N");

            // bytes32 for the contract call: keccak256(assetIdString) → hex string → raw bytes
            byte[] assetIdBytes32 = assetIdString.Keccack256().HexToByteArray();

            BigInteger subscriptionPrice = new BigInteger(500);
            BigInteger subscriptionDuration = new BigInteger(86400); // 1 day in seconds

            // --- 4. Create the asset and wait for the transaction to be mined ---
            var receipt = await registryService.CreateAssetRequestAndWaitForReceiptAsync(assetIdBytes32,
                subscriptionPrice, subscriptionDuration, TokenAddress, AssetOwnerAddress);

            // --- 5. Decode the AssetCreated event from the receipt to get the asset address ---
            AssetCreatedEventDTO createdEvent = receipt.DecodeAllEvents<AssetCreatedEventDTO>()[0].Event;

            Assert.IsNotNull(createdEvent, "AssetCreated event must be present in the receipt.");
            Assert.IsFalse(string.IsNullOrEmpty(createdEvent.AssetAddress),
                "AssetCreated event must carry the new asset contract address.");

            // --- 6. Poll the indexer until the asset is indexed or 10 seconds elapse ---
            // A fixed wait is fragile when the full test suite runs: the indexer may be
            // processing a backlog of blocks from earlier tests (AssetTests alone mines 37+
            // permanent blocks, each triggering a ClaimableRefresh handler in Ponder).
            // Polling retries on InvalidOperationException (empty items array) until the
            // asset appears, giving Ponder time to drain its backlog regardless of depth.
            AssetDto dto = default;

            int attempts = 10;

            while (attempts > 0)
            {
                await UniTask.WaitForSeconds(1f);
                try
                {
                    dto = await IndexerProvider.GetAsset(assetIdString.Keccack256(), RegistryAddress);
                    break;
                }
                catch (InvalidOperationException)
                {
                    // Asset not yet indexed — wait another second and retry.
                }

                attempts--;
            }

            Assert.IsFalse(string.IsNullOrEmpty(dto.Address.Value), "Asset was not indexed within 10 seconds (attempts).");

            // --- 8. Assert every indexed property matches what was sent to the contract ---
            Assert.IsTrue(dto.Address.Value.IsValidEthereumAddressHexFormat(),
                "Indexed asset address must be a valid Ethereum address.");

            Assert.AreEqual(createdEvent.AssetAddress.ToLower(), dto.Address.Value.ToLower(),
                "Indexed asset address must match the address emitted by the AssetCreated event.");

            Assert.AreEqual(subscriptionPrice, dto.SubscriptionPrice,
                "Indexed subscription price must match the value passed to createAsset.");

            Assert.AreEqual(TimeSpan.FromSeconds((long)subscriptionDuration), dto.SubscriptionDuration,
                "Indexed subscription duration must match the value passed to createAsset.");

            Assert.AreEqual(AssetOwnerAddress.ToLower(), dto.Owner.Value.ToLower(),
                "Indexed owner must match the owner address passed to createAsset.");

            Assert.AreEqual(TokenAddress.ToLower(), dto.TokenAddress.Value.ToLower(),
                "Indexed token address must match the token address passed to createAsset.");
        }
    }
}