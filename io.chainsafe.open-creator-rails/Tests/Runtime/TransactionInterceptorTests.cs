using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Io.ChainSafe.OpenCreatorRails;
using Io.ChainSafe.OpenCreatorRails.Contracts.Asset.ContractDefinition;
using Io.ChainSafe.OpenCreatorRails.Contracts.Asset.Service;
using Io.ChainSafe.OpenCreatorRails.Contracts.AssetRegistry.ContractDefinition;
using Io.ChainSafe.OpenCreatorRails.Utils;
using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using NUnit.Framework;

namespace Tests.Runtime
{
    /// <summary>
    /// Tests for <see cref="TransactionInterceptor"/>.
    ///
    /// The interceptor is installed by <see cref="OpenCreatorRailsService.Connect"/> on the
    /// Nethereum <c>Web3.Client.OverridingRequestInterceptor</c>. After any
    /// <c>eth_sendRawTransaction</c> or <c>eth_sendTransaction</c> call it forwards the
    /// returned transaction hash to <see cref="IEventHandler.DeduplicateEvent"/>, which adds
    /// the hash to the <see cref="PollingEventHandler"/> internal <c>_hashes</c> set so that
    /// the poll loop never re-delivers events emitted by the SDK's own transactions.
    ///
    /// These tests verify that end-to-end deduplication path: a transaction sent while the
    /// interceptor is active must not trigger the registered event delegate on the next poll,
    /// because the hash is already in <c>_hashes</c>.
    /// </summary>
    [TestFixture]
    public class TransactionInterceptorTests : TestsBase
    {
        // Pre-seeded registry from the Demo scene / seed-local.sh.
        private static readonly EthereumAddress RegistryAddress =
            new EthereumAddress("0xe7f1725E7734CE288F8367e1Bb143E90bb3F0512");

        // Pre-deployed TestToken on Anvil (nonce-0 deploy of account 0).
        private const string TokenAddress = "0x5FbDB2315678afecb367f032d93F642f64180aa3";

        private AssetService _assetService;

        private PollingEventHandler Handler =>
            (PollingEventHandler)OpenCreatorRailsService.Instance.EventHandler;

        [SetUp]
        public override async Task SetUp()
        {
            // Connect as account 0 — leaves the TransactionInterceptor active (NOT nulled,
            // unlike PollingEventHandlerTests.SetUp which deliberately removes it).
            await OpenCreatorRailsService.Instance.Connect(0);

            var web3 = OpenCreatorRailsService.Instance.Web3;
            var registryService = OpenCreatorRailsService.GetAssetRegistry(RegistryAddress);

            // Fresh asset per test so no prior event history or subscriptions interfere.
            string assetIdString = "test_interceptor_asset_" + Guid.NewGuid().ToString("N");
            byte[] assetIdBytes32 = assetIdString.Keccack256().HexToByteArray();

            var receipt = await registryService.CreateAssetRequestAndWaitForReceiptAsync(
                assetIdBytes32,
                subscriptionPrice: new BigInteger(100),
                subscriptionDuration: new BigInteger(86400),
                tokenAddress: TokenAddress,
                owner: OpenCreatorRailsService.Instance.WalletProvider.ConnectedAccount.Value);

            AssetCreatedEventDTO createdEvent = receipt.DecodeAllEvents<AssetCreatedEventDTO>()[0].Event;

            _assetService = new AssetService(web3, createdEvent.AssetAddress);
        }

        // -------------------------------------------------------------------------
        // Helper: invoke _pollEvent directly, bypassing the 12-second timer.
        // -------------------------------------------------------------------------

        private static async Task InvokePollAsync(PollingEventHandler handler)
        {
            var field = typeof(PollingEventHandler)
                .GetField("_pollEvent", BindingFlags.NonPublic | BindingFlags.Instance);

            var poll = field?.GetValue(handler) as Func<UniTask>;

            if (poll != null)
            {
                await poll.Invoke().AsTask();
            }
        }

        // -------------------------------------------------------------------------
        // Helper: read the private _hashes set via reflection.
        // -------------------------------------------------------------------------

        private static HashSet<string> GetHashes(PollingEventHandler handler)
        {
            var field = typeof(PollingEventHandler)
                .GetField("_hashes", BindingFlags.NonPublic | BindingFlags.Instance);

            return field?.GetValue(handler) as HashSet<string>;
        }

        // -------------------------------------------------------------------------
        // Test 1 — TransactionInterceptor is installed after Connect().
        //           Verifies the interceptor is wired onto the Web3 client.
        // -------------------------------------------------------------------------

        [Test]
        public void Test_Interceptor_InstalledAfterConnect()
        {
            var interceptor = OpenCreatorRailsService.Instance.Web3.Client.OverridingRequestInterceptor;

            Assert.IsNotNull(interceptor,
                "OverridingRequestInterceptor must be set after Connect().");

            Assert.IsInstanceOf<TransactionInterceptor>(interceptor,
                "OverridingRequestInterceptor must be a TransactionInterceptor instance.");
        }

        // -------------------------------------------------------------------------
        // Test 2 — A transaction hash sent while the interceptor is active is added
        //           to PollingEventHandler._hashes before the next poll runs, so the
        //           delegate is never called for that event.
        // -------------------------------------------------------------------------

        [Test]
        public async Task Test_Interceptor_DeduplicatesOwnTransaction()
        {
            bool delegateCalled = false;

            _assetService.SubscribeToEvent<SubscriptionPriceUpdatedEventDTO>(_ => delegateCalled = true);

            // Send a transaction WITH the interceptor active. The interceptor calls
            // DeduplicateEvent(hash) immediately after the RPC returns, adding the
            // hash to _hashes before InvokePollAsync runs.
            await _assetService.SetSubscriptionPriceRequestAndWaitForReceiptAsync(new BigInteger(555));

            // Poll: the event log will contain the SubscriptionPriceUpdated event, but
            // its transaction hash is already in _hashes → delegate must NOT fire.
            await InvokePollAsync(Handler);

            Assert.IsFalse(delegateCalled,
                "Delegate must not be called for a transaction sent by the SDK itself (interceptor deduplicated it).");
        }

        // -------------------------------------------------------------------------
        // Test 3 — The transaction hash is present in _hashes after the interceptor
        //           processes the send, confirming DeduplicateEvent was called.
        // -------------------------------------------------------------------------

        [Test]
        public async Task Test_Interceptor_HashAddedToDeduplicationSet()
        {
            // Send a transaction and capture the receipt to get its hash.
            var receipt = await _assetService.SetSubscriptionPriceRequestAndWaitForReceiptAsync(new BigInteger(444));

            string txHash = receipt.TransactionHash;

            HashSet<string> hashes = GetHashes(Handler);

            Assert.IsNotNull(hashes, "_hashes field must be accessible via reflection.");

            Assert.IsTrue(hashes.Contains(txHash),
                "Transaction hash must be present in PollingEventHandler._hashes after the interceptor processes the send.");
        }

        // -------------------------------------------------------------------------
        // Test 4 — An event from an EXTERNAL transaction (not sent by the SDK) IS
        //           delivered to the delegate, confirming the interceptor only
        //           deduplicates hashes it has seen, not all events.
        //           Simulated by manually removing the interceptor for one tx.
        // -------------------------------------------------------------------------

        [Test]
        public async Task Test_Interceptor_ExternalTransactionDeliveredToDelegateOnPoll()
        {
            bool delegateCalled = false;

            _assetService.SubscribeToEvent<SubscriptionPriceUpdatedEventDTO>(_ => delegateCalled = true);

            // Temporarily remove the interceptor so the hash is NOT deduplicated.
            var web3 = OpenCreatorRailsService.Instance.Web3;
            var savedInterceptor = web3.Client.OverridingRequestInterceptor;
            web3.Client.OverridingRequestInterceptor = null;

            await _assetService.SetSubscriptionPriceRequestAndWaitForReceiptAsync(new BigInteger(333));

            // Restore the interceptor.
            web3.Client.OverridingRequestInterceptor = savedInterceptor;

            // Poll: hash was NOT deduplicated → delegate must fire.
            await InvokePollAsync(Handler);

            Assert.IsTrue(delegateCalled,
                "Delegate must be called for a transaction whose hash was not deduplicated by the interceptor.");
        }
    }
}
