using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Io.ChainSafe.OpenCreatorRails;
using Io.ChainSafe.OpenCreatorRails.Utils;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.JsonRpc.Client;
using Nethereum.Util;
using NUnit.Framework;
using Random = System.Random;

namespace Tests.Runtime
{
    public class AssetTests : TestsBase
    {
        // Anvil account addresses (standard test mnemonic HD indices).
        // Account 4 has 1000 TEST allocated w/ the seed script.
        private const string Account4Address = "0x15d34AAf54267DB7D7c367839AAf71A00a2C6A65";

        // Pre-deployed TestToken on Anvil:
        // Decimals: 6.  subscriptionPrice for DefaultAsset_0 is 100 (100 micro-TEST).
        private const string TokenAddress = "0x5FbDB2315678afecb367f032d93F642f64180aa3";

        // DefaultAsset_0: owner = account 1, subscriptionPrice = 100, subscriptionDuration = 100s.
        private IAsset Asset0 => OpenCreatorRailsService.Instance.Assets[0];

        private async Task Connect(int index = 0)
        {
            // Wait for changes to be indexed before calling connect
            await UniTask.WaitForSeconds(2);
            
            // This also calls Connected(web3) on all scene assets, fully populating them from the indexer.
            await OpenCreatorRailsService.Instance.Connect(index);
        }
        
        [SetUp]
        public async Task SetUp()
        {
            // Connect as account 4 (subscriber seeded w/ 1000 TEST).
            await Connect(4);
        }

        private static string _cachedSubscriber;
        
        // ── Helper: produce a unique subscriber ID per test to prevent state
        //    collisions when Anvil persists across tests within a run ──────────

        private static string NewSubscriberId()
        {
            _cachedSubscriber = "test_subscriber_" + Guid.NewGuid().ToString("N");

            return _cachedSubscriber;
        }

        // Helper method to warp time on Anvil
        /// Push time forward by `seconds` relative to the current chain time.
        public async Task WarpTime(long seconds)
        {
            await OpenCreatorRailsService.Instance.Web3.Client.SendRequestAsync(new RpcRequest(1, "evm_increaseTime", seconds));
            
            await OpenCreatorRailsService.Instance.Web3.Client.SendRequestAsync(new RpcRequest(1, "evm_mine", 1));
        }
        
        [Test]
        public void Test_IsAssetIdHashValid()
        {
            Assert.AreEqual(Asset0.AssetIdHash, Asset0.AssetId.Keccack256());
        }
        
        [Test]
        public void Test_Connected_AddressIsValid()
        {
            Assert.IsTrue(Asset0.Address.Value.IsValidEthereumAddressHexFormat(),
                "Asset.Address must be a valid Ethereum address after Connected().");
        }

        [Test]
        public void Test_Connected_SubscriptionPriceIsValid()
        {
            Assert.Greater(Asset0.SubscriptionPrice, BigInteger.Zero,
                "Asset.SubscriptionPrice must be > 0 after Connected().");
            
            // Multiple of 100
            Assert.AreEqual(Asset0.SubscriptionPrice % 100, BigInteger.Zero);
        }

        [Test]
        public void Test_Connected_SubscriptionDurationIsValid()
        {
            Assert.Greater(Asset0.SubscriptionDuration, TimeSpan.Zero,
                "Asset.SubscriptionDuration must be > TimeSpan.Zero after Connected().");
            
            Assert.AreEqual(Asset0.SubscriptionDuration.TotalSeconds, 100);
        }

        [Test]
        public void Test_Connected_OwnerIsValidAddress()
        {
            Assert.IsTrue(Asset0.Owner.Value.IsValidEthereumAddressHexFormat(),
                "Asset.Owner must be a valid Ethereum address after Connected().");
            
            Assert.AreEqual(Asset0.Owner.Value.ToLower(), "0x70997970C51812dc3A010C7d01b50e0d17dc79C8".ToLower());
        }

        [Test]
        public void Test_Connected_TokenAddressIsValidAddress()
        {
            Assert.IsTrue(Asset0.TokenAddress.Value.IsValidEthereumAddressHexFormat(),
                "Asset.TokenAddress must be a valid Ethereum address after Connected().");
    }

        [Test]
        public void Test_Connected_SubscriptionsIsNotNull()
        {
            Assert.IsNotNull(Asset0.Subscriptions,
                "Asset.Subscriptions must not be null after Connected().");
        }

        [Test]
        public void Test_Connected_ServiceIsValid()
        {
            Assert.IsNotNull(Asset0.Service,
                "Asset.Service must not be null after Connected().");
            
            Assert.AreEqual(Asset0.Service.ContractAddress.ToLower(), Asset0.Address.Value.ToLower());
        }

        [Test]
        public void Test_Connected_PermitServiceIsValid()
        {
            Assert.IsNotNull(Asset0.PermitService,
                "Asset.PermitService must not be null after Connected().");
            
            Assert.AreEqual(Asset0.PermitService.ContractAddress.ToLower(), Asset0.TokenAddress.Value.ToLower());
        }
        
        [Test]
        public void Test_Connected_AssetRegistryServiceIsNotNull()
        {
            Assert.IsNotNull(Asset0.AssetRegistryService,
                "Asset.AssetRegistryService must not be null after Connected().");
            
            Assert.AreEqual(Asset0.AssetRegistryService.ContractAddress.ToLower(), Asset0.RegistryAddress.Value.ToLower());
        }
        
        [Test]
        public async Task Test_GetSubscriptionPriceAndDuration_ReturnsPositiveValues()
        {
            var (price, duration) = await Asset0.GetSubscriptionPriceAndDuration(new BigInteger(1));

            Assert.Greater(price, BigInteger.Zero,
                "GetSubscriptionPriceAndDuration price must be > 0.");

            Assert.Greater(duration, TimeSpan.Zero,
                "GetSubscriptionPriceAndDuration duration must be > TimeSpan.Zero.");
        }

        [Test]
        public async Task Test_Subscribe_Expiration()
        {
            string subscriberId = NewSubscriberId();

            DateTime expiry = await Asset0.Subscribe(subscriberId, new BigInteger(1));

            Assert.Greater(expiry, DateTime.Now,
                "Subscribe must return an expiry date in the future.");

            var fetchedExpiry = await Asset0.GetSubscriptionExpiration(subscriberId);
            
            Assert.AreEqual(expiry, fetchedExpiry);
        }
        
        [Test]
        public async Task Test_Subscribe_IsExpired()
        {
            string subscriberId = NewSubscriberId();

            bool isExpired = await Asset0.IsSubscriptionExpired(subscriberId);
            
            Assert.IsTrue(isExpired, "IsSubscriptionExpired failed.");
            
            await Asset0.Subscribe(subscriberId, new BigInteger(1));

            isExpired = await Asset0.IsSubscriptionExpired(subscriberId);
            
            Assert.IsFalse(isExpired);
        }
        
        [Test]
        public async Task Test_Subscribe_IsActive()
        {
            string subscriberId = NewSubscriberId();

            bool isActive = await Asset0.IsSubscriptionActive(subscriberId);
            
            Assert.IsFalse(isActive, "IsSubscriptionActive failed.");
            
            await Asset0.Subscribe(subscriberId, new BigInteger(1));

            isActive = await Asset0.IsSubscriptionActive(subscriberId);
            
            Assert.IsTrue(isActive);
        }
        
        [Test]
        public async Task Test_GetSubscriptionPriceAndDuration()
        {
            (BigInteger price, TimeSpan duration) priceDurationPair =  await Asset0.GetSubscriptionPriceAndDuration(new BigInteger(1));

            int multiple = 17;
            
            (BigInteger price, TimeSpan duration) multiplePriceDurationPair =  await Asset0.GetSubscriptionPriceAndDuration(new BigInteger(multiple));
            
            Assert.AreEqual(priceDurationPair.price * multiple, multiplePriceDurationPair.price);
            Assert.AreEqual(priceDurationPair.duration * multiple, multiplePriceDurationPair.duration);
            
            Assert.AreEqual(priceDurationPair.price, Asset0.SubscriptionPrice);

            // Asset Owner
            await Connect(1);
            await Asset0.SetSubscriptionPrice(Asset0.SubscriptionPrice * multiple);

            (BigInteger price, TimeSpan duration) newPriceDurationPair = await Asset0.GetSubscriptionPriceAndDuration(new BigInteger(1));
            
            Assert.AreEqual(multiplePriceDurationPair.price, newPriceDurationPair.price);
            Assert.AreEqual(priceDurationPair.duration, newPriceDurationPair.duration);
        }

        [Test]
        public async Task Test_SubscriptionAddition()
        {
            string subscriberId = NewSubscriberId();

            DateTime expiry = await Asset0.Subscribe(subscriberId, new BigInteger(1));

            string subscribersIdHash = subscriberId.ToSubscriberIdHash().ToHex(true);
            
            var subscriptions = Asset0.Subscriptions.Where(subscription => subscription.SubscriberIdHash == subscribersIdHash).ToList();
            
            Assert.AreEqual(subscriptions.Count(), 1);
            
            var subscription = subscriptions.First();
            
            Assert.AreEqual(subscription.Nonce, BigInteger.Zero);
            Assert.AreEqual(subscription.EndTime, expiry);
            Assert.AreEqual(subscription.SubscriptionPrice, Asset0.SubscriptionPrice);
            Assert.AreEqual(subscription.RegistryFeeShare, await Asset0.AssetRegistryService.GetRegistryFeeShareQueryAsync());
            Assert.AreEqual(subscription.Payer.Value, Account4Address);
            Assert.AreEqual(subscription.IsExpired, false);
            Assert.AreEqual(subscription.IsRevoked, false);
            Assert.AreEqual(subscription.IsActive, true);
        }
        
        [Test]
        public async Task Test_SubscriptionExtension()
        {
            string subscriberId = NewSubscriberId();

            await Asset0.Subscribe(subscriberId, new BigInteger(1));
            
            string subscribersIdHash = subscriberId.ToSubscriberIdHash().ToHex(true);
            
            var previousSubscription = Asset0.Subscriptions.First(subscription => subscription.SubscriberIdHash == subscribersIdHash);
            
            DateTime extendedExpiry = await Asset0.Subscribe(subscriberId, new BigInteger(2));
            
            var subscriptions = Asset0.Subscriptions.Where(subscription => subscription.SubscriberIdHash == subscribersIdHash).ToList();
            
            Assert.AreEqual(subscriptions.Count(), 1);
            
            var subscription = subscriptions.First();
            
            Assert.AreEqual(subscription.Nonce, BigInteger.Zero);
            Assert.AreEqual(subscription.EndTime, extendedExpiry);
            
            Assert.AreEqual(previousSubscription.EndTime + (Asset0.SubscriptionDuration * 2), subscription.EndTime);
        }
        
        [Test]
        public async Task Test_SubscriptionRenewal()
        {
            string subscriberId = NewSubscriberId();

            DateTime expiry = await Asset0.Subscribe(subscriberId, new BigInteger(1));

            string subscribersIdHash = subscriberId.ToSubscriberIdHash().ToHex(true);

            // Asset Owner
            await Connect(1);
            await Asset0.SetSubscriptionPrice(new BigInteger(new Random().Next(1, 101) * 100));

            // Subscriber
            await Connect(4);
            DateTime newExpiry = await Asset0.Subscribe(subscriberId, new BigInteger(1));

            var subscriptions = Asset0.Subscriptions.Where(subscription => subscription.SubscriberIdHash == subscribersIdHash).ToList();
            
            Assert.AreEqual(2, subscriptions.Count);
            Assert.AreEqual(BigInteger.Zero, subscriptions.First().Nonce);
            
            var subscription = subscriptions.Last();
            
            Assert.AreEqual(subscription.Nonce, BigInteger.One);
            Assert.AreEqual(subscription.StartTime, expiry);
            Assert.AreEqual(subscription.EndTime, newExpiry);
        }
        
        [Test]
        public async Task Test_CancelSubscription()
        {
            string subscriberId = NewSubscriberId();

            DateTime expiry = await Asset0.Subscribe(subscriberId, new BigInteger(3));

            // Wait for subscription to pass, so block.timestamp != subscription.startTime
            await UniTask.WaitForSeconds(2f);
                
            await Asset0.CancelSubscription(subscriberId);
            
            DateTime newExpiry = await Asset0.GetSubscriptionExpiration(subscriberId);
            
            Assert.AreEqual(newExpiry, expiry - (Asset0.SubscriptionDuration * 2));
        }
        
        [Test]
        public async Task Test_CancelSubscription_NewExpiration()
        {
            string subscriberId = NewSubscriberId();

            DateTime expiry = await Asset0.Subscribe(subscriberId, new BigInteger(3));
            
            // Wait for subscription to pass, so block.timestamp != subscription.startTime
            await UniTask.WaitForSeconds(2f);

            await Asset0.CancelSubscription(subscriberId);
            
            string subscribersIdHash = _cachedSubscriber.ToSubscriberIdHash().ToHex(true);

            var subscription = Asset0.Subscriptions.First(subscription => subscription.SubscriberIdHash == subscribersIdHash);
            
            Assert.AreEqual(subscription.EndTime, expiry - (Asset0.SubscriptionDuration * 2));
        }
        
        [Test]
        public async Task Test_CancelSubscription_NewNonce()
        {
            // Subscribe for 1 period
            // Connect to Asset Owner
            // Set New (Random) Subscription Price
            // Subscribe again for 1 Period (new nonce)
            // Cancel Subscription
            // Assert that there's only one subscription matching this subscriber in Asset0.Subscriptions and it's Nonce == 0
        }
        
        [Test]
        public async Task Test_CancelSubscription_Refund()
        {
            // Get balance of token in connected account
            // Subscribe for 3 periods
            // Get balance of token in connected account again
            // Assert 3x subscription price was deducted
            // Wait for 2 seconds
            // Cancel Subscription
            // Get balance of token in connected account again
            // Check if 2x subscription price was refunded
        }
        
        [Test]
        public async Task Test_SetSubscriptionPrice()
        {
            BigInteger originalPrice = Asset0.SubscriptionPrice;

            // Asset Owner
            await Connect(1);
            await Asset0.SetSubscriptionPrice(new BigInteger(999));

            Assert.AreEqual(new BigInteger(999), Asset0.SubscriptionPrice,
                "Asset.SubscriptionPrice must reflect the new value after SetSubscriptionPrice.");

            BigInteger fetchedPrice = await Asset0.Service.GetSubscriptionPriceQueryAsync(1);
            
            Assert.AreEqual(fetchedPrice, new BigInteger(999));
            
            // Restore so subsequent tests that depend on SubscriptionPrice = 1 are unaffected.
            await Asset0.SetSubscriptionPrice(originalPrice);
        }

        [Test]
        public async Task Test_ClaimCreatorFeeShare_Single()
        {
            // Get and cache Creator's Token Balance
            // Subscribe for 3 periods
            // Warp time to 3 periods
            // Switch/Connect to Asset Owner
            // claimCreatorFee
            // Assert claimed/retruned amount == 3x subscriptionPrice
            // Assert Owner's Token Balance has increased by 3x subscriptionPrice
        }
        
        [Test]
        public async Task Test_ClaimCreatorFeeShare_Batch()
        {
            // Get and cache Creator's Token Balance
            // Subscribe for 1, 2 and 3 periods for 3 subscribers
            // Warp time to 3 periods
            // Switch/Connect to Asset Owner
            // claimCreatorFee batch
            // Assert claimed/retruned amount == 6x subscriptionPrice
            // Assert Owner's Token Balance has increased by 6x subscriptionPrice
        }
        
        [Test]
        public async Task Test_RevokeSubscription()
        {
            // Subscribe for 1 period
            // Switch/Connect to Asset Owner
            // Wait for 2 seconds
            // Revoke subscription
            // Fetch current expiration
            // Assert current expiry is less than subscription expiry
        }
        
        [Test]
        public async Task Test_RevokeSubscription_IsRevoked()
        {
            // Subscribe for 1 period
            // Assert isRevoked for subscriber is false
            // Switch/Connect to Asset Owner
            // Wait for 2 seconds
            // Revoke subscription
            // Assert isRevoked for subscriber is true
        }
        
        [Test]
        public async Task Test_RevokeSubscription_IsActive()
        {
            // Subscribe for 1 period
            // Assert isActive for subscriber is true
            // Switch/Connect to Asset Owner
            // Wait for 2 seconds
            // Revoke subscription
            // Assert isActive for subscriber is false
        }
        
        [Test]
        public async Task Test_RevokeSubscription_TrySubscribe()
        {
            // Subscribe for 1 period
            // Switch/Connect to Asset Owner
            // Wait for 2 seconds
            // Revoke subscription
            // Try to subscribe and Assert that it throws OnlyUnrevokedUnauthorizedSubscriberError
            // Catch a SmartContractCustomErrorRevertException and use Asset0.Service.FindCustomErrorException to decode it
        }
        
        [Test]
        public async Task Test_RevokeSubscription_TryCancel()
        {
            // Subscribe for 1 period
            // Switch/Connect to Asset Owner
            // Wait for 2 seconds
            // Revoke subscription
            // Try to cancel subscription and Assert that it throws OnlyUnrevokedUnauthorizedSubscriberError
            // Catch a SmartContractCustomErrorRevertException and use Asset0.Service.FindCustomErrorException to decode it
        }
        
        [Test]
        public async Task Test_RevokeSubscription_NewExpiration()
        {
            // Subscribe for fetched current 1 period
            // Switch/Connect to Asset Owner
            // Wait for 2 seconds
            // Revoke subscription
            // Assert current expiry in Asset0.Subscriptions is less than the subscribing expiry
        }
        
        [Test]
        public async Task Test_RevokeSubscription_NewNonce()
        {
            // Subscribe for 1 period
            // Connect to Asset Owner
            // Set New (Random) Subscription Price
            // Subscribe again for 1 Period (new nonce)
            // Wait for 2 seconds
            // Revoke Subscription
            // Assert that there's only one subscription matching this subscriber in Asset0.Subscriptions and its Nonce == 0
        }
        
        [Test]
        public async Task Test_RevokeSubscription_Refund()
        {
            // Get balance of token in connected account
            // Subscribe for 3 periods
            // Get balance of token in connected account again
            // Assert 3x subscription price was deducted
            // Wait for 2 seconds
            // Revoke Subscription
            // Get balance of token in connected account again
            // Check if > 2x subscription price was refunded
        }
        
        [Test]
        public async Task Test_UnrevokeSubscription_IsRevoked()
        {
            // Subscribe for 1 period
            // Assert isRevoked is false
            // Switch/Connect to Asset Owner
            // Wait for 2 seconds
            // Revoke subscription
            // Assert isRevoked is true
            // UnrevokeSubscription
            // Assert isRevoked is false again
        }
        
        [Test]
        public async Task Test_UnrevokeSubscription_IsActive()
        {
            // Subscribe for 1 period
            // Assert isActive is true
            // Switch/Connect to Asset Owner
            // Wait for 2 seconds
            // Revoke subscription
            // Assert isActive is false
            // UnrevokeSubscription
            // Assert isActive is true again
        }
        
        [Test]
        public async Task Test_UnrevokeSubscription_TrySubscribe()
        {
            // Subscribe for 1 period
            // Switch/Connect to Asset Owner
            // Wait for 2 seconds
            // Revoke subscription
            // Assert isRevoked is true
            // UnrevokeSubscription
            // Assert Subscribing again doesn't throw
        }
        
        [Test]
        public async Task Test_UnrevokeSubscription_TryCancel()
        {
            // Subscribe for 1 period
            // Switch/Connect to Asset Owner
            // Wait for 2 seconds
            // Revoke subscription
            // Assert isRevoked is true
            // UnrevokeSubscription
            // Assert Cancelling subscription doesn't throw
        }
    }
}
