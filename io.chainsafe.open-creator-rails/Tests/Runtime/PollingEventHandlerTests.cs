using System;
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
    [TestFixture]
    public class PollingEventHandlerTests : TestsBase
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
            // Connect as account 1 — owner of the pre-seeded registry.
            await OpenCreatorRailsService.Instance.Connect(0);

            var web3 = OpenCreatorRailsService.Instance.Web3;
            // There's an implemented TransactionInterceptor assigned to this during connect
            // that deduplicates events so we set it to null for testing
            web3.Client.OverridingRequestInterceptor = null;
            var registryService = OpenCreatorRailsService.GetAssetRegistry(RegistryAddress);

            // Append a Guid so repeated SetUp calls never hit the AssetAlreadyExists revert.
            string assetIdString = "test_event_handler_asset_" + Guid.NewGuid().ToString("N");
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
        // Helper: invoke the internal _pollEvent delegate directly, bypassing the
        // 12-second timer, so tests are deterministic and require no waiting.
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
        // Helper: overwrite the private _lastBlock field via reflection.
        // -------------------------------------------------------------------------

        private static void SetLastBlock(PollingEventHandler handler, BigInteger value)
        {
            var field = typeof(PollingEventHandler)
                .GetField("_lastBlock", BindingFlags.NonPublic | BindingFlags.Instance);

            field?.SetValue(handler, value);
        }

        // -------------------------------------------------------------------------
        // Test 1 — Subscribe registers without throwing; _pollEvent is non-null
        //           after at least one Subscribe call has been made.
        // -------------------------------------------------------------------------

        [Test]
        public void Test_Subscribe_RegistersWithoutThrowing()
        {
            Assert.IsNotNull(Handler, "EventHandler must be a PollingEventHandler instance.");

            // Register a subscription using the extension method — must not throw.
            _assetService.SubscribeToEvent<SubscriptionPriceUpdatedEventDTO>(_ => { });

            var field = typeof(PollingEventHandler)
                .GetField("_pollEvent", BindingFlags.NonPublic | BindingFlags.Instance);

            var poll = field?.GetValue(Handler) as Func<UniTask>;

            Assert.IsNotNull(poll, "_pollEvent must be non-null after Subscribe is called.");
        }

        // -------------------------------------------------------------------------
        // Test 2 — After subscribing and emitting an event on-chain, one poll cycle
        //           delivers the event to the registered delegate with the correct payload.
        // -------------------------------------------------------------------------

        [Test]
        public async Task Test_Subscribe_EventDelivered_OnPoll()
        {
            SubscriptionPriceUpdatedEventDTO receivedEvent = null;

            _assetService.SubscribeToEvent<SubscriptionPriceUpdatedEventDTO>(dto => receivedEvent = dto);

            await _assetService.SetSubscriptionPriceRequestAndWaitForReceiptAsync(new BigInteger(999));

            await InvokePollAsync(Handler);

            Assert.IsNotNull(receivedEvent, "Delegate must be invoked after the event is emitted and the poll fires.");
            Assert.AreEqual(new BigInteger(999), receivedEvent.NewSubscriptionPrice,
                "Delivered event payload must match the value passed to setSubscriptionPrice.");
        }

        // -------------------------------------------------------------------------
        // Test 3 — The same event is not delivered twice when the same block range
        //           is covered by two consecutive poll cycles (deduplication by tx hash).
        // -------------------------------------------------------------------------

        [Test]
        public async Task Test_Subscribe_EventDeduplication()
        {
            var web3 = OpenCreatorRailsService.Instance.Web3;
            int invokeCount = 0;

            _assetService.SubscribeToEvent<SubscriptionPriceUpdatedEventDTO>(_ => invokeCount++);

            await _assetService.SetSubscriptionPriceRequestAndWaitForReceiptAsync(new BigInteger(777));

            // First poll: event is new — delegate fires, tx hash recorded in _hashes.
            await InvokePollAsync(Handler);

            Assert.AreEqual(1, invokeCount, "Delegate must be called exactly once after the first poll.");

            // Rewind _lastBlock to (currentBlock - 1) so the next poll covers the same
            // block range again. currentBlock - _lastBlock is guaranteed > 0, so the
            // early-return guard is bypassed and the _hashes deduplication set is exercised.
            BigInteger currentBlock = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            SetLastBlock(Handler, currentBlock - 1);

            // Second poll: same tx hash already in _hashes — delegate must NOT fire again.
            await InvokePollAsync(Handler);

            Assert.AreEqual(1, invokeCount, "Delegate must not be called a second time for the same transaction hash.");
        }

        // -------------------------------------------------------------------------
        // Test 4 — If no events are emitted, a poll cycle does not invoke the delegate.
        // -------------------------------------------------------------------------

        [Test]
        public async Task Test_Subscribe_NoEventsInRange_DelegateNotCalled()
        {
            bool delegateCalled = false;

            _assetService.SubscribeToEvent<SubscriptionPriceUpdatedEventDTO>(_ => delegateCalled = true);

            // No on-chain emission — poll immediately.
            await InvokePollAsync(Handler);

            Assert.IsFalse(delegateCalled, "Delegate must not be called when no events were emitted in the polled range.");
        }

        // -------------------------------------------------------------------------
        // Test 5 — After unsubscribing, a subsequent poll cycle does not invoke the
        //           removed delegate even when a matching event is emitted.
        // -------------------------------------------------------------------------

        [Test]
        public async Task Test_Unsubscribe_DelegateNotCalledAfterUnsubscribe()
        {
            int invokeCount = 0;

            EventDelegate<SubscriptionPriceUpdatedEventDTO> handler = _ => invokeCount++;

            // Subscribe and confirm delivery works.
            _assetService.SubscribeToEvent<SubscriptionPriceUpdatedEventDTO>(handler);
            await _assetService.SetSubscriptionPriceRequestAndWaitForReceiptAsync(new BigInteger(111));
            await InvokePollAsync(Handler);

            Assert.AreEqual(1, invokeCount, "Delegate must be invoked once after the first emission.");

            // Unsubscribe — the delegate must no longer receive events.
            _assetService.UnsubscribeToEvent<SubscriptionPriceUpdatedEventDTO>(handler);

            await _assetService.SetSubscriptionPriceRequestAndWaitForReceiptAsync(new BigInteger(222));
            await InvokePollAsync(Handler);

            Assert.AreEqual(1, invokeCount, "Delegate must not be called after UnsubscribeToEvent.");
        }
    }
}
