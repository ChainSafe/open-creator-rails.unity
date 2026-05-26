using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Io.ChainSafe.OpenCreatorRails;
using Io.ChainSafe.OpenCreatorRails.Contracts.Asset.ContractDefinition;
using Io.ChainSafe.OpenCreatorRails.Utils;
using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.JsonRpc.Client;
using Nethereum.Util;
using NUnit.Framework;
using UnityEngine;
using Random = System.Random;

namespace Tests.Runtime
{
    [TestFixture]
    public class AssetTests : TestsBase
    {
        // Anvil account addresses (standard test mnemonic HD indices).
        // Account 1 is the owner of DefaultAsset_0.
        private const string Account1Address = "0x70997970C51812dc3A010C7d01b50e0d17dc79C8";
        // Account 5 - 11 are seeded w/ 1000 TEST tokens.
        private const string Account5Address = "0x9965507D1a55bcC2695C58ba16FB37d819B0A4dc";
        
        // DefaultAsset_0: owner = account 1, subscriptionPrice = 100, subscriptionDuration = 100s.
        private IAsset Asset0 => OpenCreatorRailsService.Instance.Assets[0];

        [SetUp]
        public override async Task SetUp()
        {
            await base.SetUp();
            
            // Connect as account 5 (subscriber seeded w/ 1000 TEST).
            await OpenCreatorRailsService.Instance.Connect(5);
        }

        [TearDown]
        public override async Task TearDown()
        {
            await base.TearDown();

            // Asset Owner
            await OpenCreatorRailsService.Instance.Connect(1);
            // Reset Subscription Price
            await Asset0.SetSubscriptionPrice(new BigInteger(100));
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
        private async Task WarpTime(long seconds)
        {
            await OpenCreatorRailsService.Instance.Web3.Client.SendRequestAsync(new RpcRequest(1, "evm_increaseTime", seconds));

            await OpenCreatorRailsService.Instance.Web3.Client.SendRequestAsync(new RpcRequest(1, "evm_mine"));
        }
        
        [Test]
        public void Test_Connected_TokenSymbolIsValid()
        {
            // TokenSymbol is populated in Connected() via PermitService.SymbolQueryAsync().
            // TestToken.sol defines SYMBOL = "TEST".
            Assert.AreEqual("TEST", Asset0.TokenSymbol,
                "Asset.TokenSymbol must equal the on-chain ERC-20 symbol after Connected().");
        }

        [Test]
        public void Test_Connected_TokenDecimalsIsValid()
        {
            // TokenDecimals is populated in Connected() via PermitService.DecimalsQueryAsync().
            // TestToken.sol overrides decimals() to return 6.
            Assert.AreEqual(new BigInteger(6), Asset0.TokenDecimals,
                "Asset.TokenDecimals must equal 6 (TestToken override) after Connected().");
        }

        [Test]
        public void Test_SubscriptionPricePrimaryUnit_MatchesExpected()
        {
            // SubscriptionPricePrimaryUnit is a default interface method:
            //   (decimal)SubscriptionPrice / TokenDecimals.PowerOfTen()
            // DefaultAsset_0: SubscriptionPrice = 100, TokenDecimals = 6
            //   → 100 / 10^6 = 0.0001
            decimal expected = (decimal)Asset0.SubscriptionPrice / Asset0.TokenDecimals.PowerOfTen();

            Assert.AreEqual(expected, Asset0.SubscriptionPricePrimaryUnit,
                "SubscriptionPricePrimaryUnit must equal SubscriptionPrice / 10^TokenDecimals.");

            // Sanity-check against the concrete value given the seeded asset configuration.
            Assert.AreEqual(0.0001m, Asset0.SubscriptionPricePrimaryUnit,
                "SubscriptionPricePrimaryUnit for DefaultAsset_0 must be 0.0001 (100 / 10^6).");
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
            await OpenCreatorRailsService.Instance.Connect(1);
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
            Assert.AreEqual(subscription.Payer.Value, Account5Address);
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
            await OpenCreatorRailsService.Instance.Connect(1);
            await Asset0.SetSubscriptionPrice(new BigInteger(new Random().Next(1, 101) * 100));

            // Subscriber
            await OpenCreatorRailsService.Instance.Connect(5);
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
            await UniTask.WaitForSeconds(1f);
                
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
            await UniTask.WaitForSeconds(1f);

            await Asset0.CancelSubscription(subscriberId);
            
            string subscribersIdHash = _cachedSubscriber.ToSubscriberIdHash().ToHex(true);

            var subscription = Asset0.Subscriptions.First(subscription => subscription.SubscriberIdHash == subscribersIdHash);
            
            Assert.AreEqual(subscription.EndTime, expiry - (Asset0.SubscriptionDuration * 2));
        }
        
        [Test]
        public async Task Test_CancelSubscription_NewNonce()
        {
            // Subscribe for 1 period
            string subscriberId = NewSubscriberId();
            await Asset0.Subscribe(subscriberId, new BigInteger(1));

            // Connect to Asset Owner
            await OpenCreatorRailsService.Instance.Connect(1);
            // Set New (Random) Subscription Price
            try
            {
                await Asset0.SetSubscriptionPrice(new BigInteger(new Random().Next(1, 101) * 100));
            }
            catch (SmartContractCustomErrorRevertException e)
            {
                Debug.LogError(Asset0.Service.FindCustomErrorException(e).DecodedError);
                throw;
            }

            // Subscribe again for 1 Period (new nonce)
            await OpenCreatorRailsService.Instance.Connect(5);
            await Asset0.Subscribe(subscriberId, new BigInteger(1));

            // Wait for 1 second
            await UniTask.WaitForSeconds(1f);
            // Cancel Subscription
            await Asset0.CancelSubscription(subscriberId);

            // Assert that there's only one subscription matching this subscriber in Asset0.Subscriptions and it's Nonce == 0
            string subscribersIdHash = subscriberId.ToSubscriberIdHash().ToHex(true);
            var subscriptions = Asset0.Subscriptions.Where(s => s.SubscriberIdHash == subscribersIdHash).ToList();
            Assert.AreEqual(1, subscriptions.Count);
            Assert.AreEqual(BigInteger.Zero, subscriptions.First().Nonce);
        }
        
        [Test]
        public async Task Test_CancelSubscription_Refund()
        {
            // Get balance of token in connected account
            BigInteger balanceBefore = await Asset0.PermitService.BalanceOfQueryAsync(Account5Address);

            // Subscribe for 3 periods
            string subscriberId = NewSubscriberId();
            await Asset0.Subscribe(subscriberId, new BigInteger(3));

            // Get balance of token in connected account again
            BigInteger balanceAfterSubscribe = await Asset0.PermitService.BalanceOfQueryAsync(Account5Address);

            // Assert 3x subscription price was deducted
            Assert.AreEqual(balanceBefore - balanceAfterSubscribe, Asset0.SubscriptionPrice * 3);

            // Wait for 1 second
            await UniTask.WaitForSeconds(1f);

            // Cancel Subscription
            await Asset0.CancelSubscription(subscriberId);

            // Get balance of token in connected account againte
            BigInteger balanceAfterCancel = await Asset0.PermitService.BalanceOfQueryAsync(Account5Address);

            // Check if 2x subscription price was refunded
            Assert.AreEqual(balanceAfterCancel - balanceAfterSubscribe, Asset0.SubscriptionPrice * 2);
        }
        
        [Test]
        public async Task Test_SetSubscriptionPrice()
        {
            BigInteger originalPrice = Asset0.SubscriptionPrice;

            // Asset Owner
            await OpenCreatorRailsService.Instance.Connect(1);
            await Asset0.SetSubscriptionPrice(new BigInteger(900));

            Assert.AreEqual(new BigInteger(900), Asset0.SubscriptionPrice,
                "Asset.SubscriptionPrice must reflect the new value after SetSubscriptionPrice.");

            BigInteger fetchedPrice = await Asset0.Service.GetSubscriptionPriceQueryAsync(1);
            
            Assert.AreEqual(fetchedPrice, new BigInteger(900));
            
            // Restore so subsequent tests that depend on SubscriptionPrice = 1 are unaffected.
            await Asset0.SetSubscriptionPrice(originalPrice);
        }

        [Test]
        public async Task Test_ClaimCreatorFeeShare_Single()
        {
            // Get and cache Creator's Token Balance
            BigInteger ownerBalanceBefore = await Asset0.PermitService.BalanceOfQueryAsync(Account1Address);

            // Subscribe for 3 periods
            await OpenCreatorRailsService.Instance.Connect(5);
            string subscriberId = NewSubscriberId();
            await Asset0.Subscribe(subscriberId, new BigInteger(3));

            // Warp time to 3 periods
            await UniTask.WaitForSeconds(1f);
            await WarpTime((long)Asset0.SubscriptionDuration.TotalSeconds * 3);

            // Switch/Connect to Asset Owner
            await OpenCreatorRailsService.Instance.Connect(1);

            // claimCreatorFee
            BigInteger claimed = await Asset0.ClaimCreatorFee(subscriberId, new EthereumAddress(Account5Address));

            // Assert claimed/returned amount > 0
            Assert.Greater(claimed, BigInteger.Zero);

            // Assert Owner's Token Balance has increased by claimed amount
            BigInteger ownerBalanceAfter = await Asset0.PermitService.BalanceOfQueryAsync(Account1Address);
            Assert.AreEqual(ownerBalanceBefore + claimed, ownerBalanceAfter);
        }
        
        [Test]
        public async Task Test_ClaimCreatorFeeShare_Batch()
        {
            // Get and cache Creator's Token Balance
            BigInteger ownerBalanceBefore = await Asset0.PermitService.BalanceOfQueryAsync(Account1Address);

            // Subscribe for 1, 2 and 3 periods for 3 subscribers
            await OpenCreatorRailsService.Instance.Connect(5);
            string subscriberId1 = NewSubscriberId();
            await Asset0.Subscribe(subscriberId1, new BigInteger(1));
            string subscriberId2 = NewSubscriberId();
            await Asset0.Subscribe(subscriberId2, new BigInteger(2));
            string subscriberId3 = NewSubscriberId();
            await Asset0.Subscribe(subscriberId3, new BigInteger(3));

            // Warp time to 3 periods
            await UniTask.WaitForSeconds(1f);
            await WarpTime((long)Asset0.SubscriptionDuration.TotalSeconds * 3);

            // Switch/Connect to Asset Owner
            await OpenCreatorRailsService.Instance.Connect(1);

            // claimCreatorFee batch
            var subscriberAddress = new EthereumAddress(Account5Address);
            BigInteger total = await Asset0.ClaimCreatorFee(new (string, EthereumAddress)[]
            {
                (subscriberId1, subscriberAddress),
                (subscriberId2, subscriberAddress),
                (subscriberId3, subscriberAddress),
            });

            // Assert claimed/returned amount > 0
            Assert.Greater(total, BigInteger.Zero);

            // Assert Owner's Token Balance has increased by total claimed amount
            BigInteger ownerBalanceAfter = await Asset0.PermitService.BalanceOfQueryAsync(Account1Address);
            Assert.AreEqual(ownerBalanceBefore + total, ownerBalanceAfter);
        }
        
        [Test]
        public async Task Test_RevokeSubscription()
        {
            // Subscribe for 1 period
            string subscriberId = NewSubscriberId();
            DateTime expiry = await Asset0.Subscribe(subscriberId, new BigInteger(1));

            // Switch/Connect to Asset Owner
            await OpenCreatorRailsService.Instance.Connect(1);

            // Wait for 1 second
            await UniTask.WaitForSeconds(1f);

            // Revoke subscription
            await Asset0.RevokeSubscription(subscriberId, new EthereumAddress(Account5Address));

            // Fetch current expiration (connected as account 4 so ToSubscriberIdHash uses correct address)
            await OpenCreatorRailsService.Instance.Connect(5);
            DateTime currentExpiry = await Asset0.GetSubscriptionExpiration(subscriberId);

            // Assert current expiry is less than subscription expiry
            Assert.Less(currentExpiry, expiry);
        }
        
        [Test]
        public async Task Test_RevokeSubscription_IsRevoked()
        {
            // Subscribe for 1 period
            string subscriberId = NewSubscriberId();
            await Asset0.Subscribe(subscriberId, new BigInteger(1));

            // Assert isRevoked for subscriber is false
            Assert.IsFalse(await Asset0.IsSubscriberRevoked(subscriberId));

            // Switch/Connect to Asset Owner
            await OpenCreatorRailsService.Instance.Connect(1);

            // Wait for 1 second
            await UniTask.WaitForSeconds(1f);

            // Revoke subscription
            await Asset0.RevokeSubscription(subscriberId, new EthereumAddress(Account5Address));

            // Assert isRevoked for subscriber is true (connected as account 4 for correct hash)
            await OpenCreatorRailsService.Instance.Connect(5);
            Assert.IsTrue(await Asset0.IsSubscriberRevoked(subscriberId));
        }
        
        [Test]
        public async Task Test_RevokeSubscription_IsActive()
        {
            // Subscribe for 1 period
            string subscriberId = NewSubscriberId();
            await Asset0.Subscribe(subscriberId, new BigInteger(1));

            // Assert isActive for subscriber is true
            Assert.IsTrue(await Asset0.IsSubscriptionActive(subscriberId));

            // Switch/Connect to Asset Owner
            await OpenCreatorRailsService.Instance.Connect(1);

            // Wait for 1 second
            await UniTask.WaitForSeconds(1f);

            // Revoke subscription
            await Asset0.RevokeSubscription(subscriberId, new EthereumAddress(Account5Address));

            // Assert isActive for subscriber is false (connected as account 4 for correct hash)
            await OpenCreatorRailsService.Instance.Connect(5);
            Assert.IsFalse(await Asset0.IsSubscriptionActive(subscriberId));
        }
        
        [Test]
        public async Task Test_RevokeSubscription_TrySubscribe()
        {
            // Subscribe for 1 period
            string subscriberId = NewSubscriberId();
            await Asset0.Subscribe(subscriberId, new BigInteger(1));

            // Switch/Connect to Asset Owner
            await OpenCreatorRailsService.Instance.Connect(1);

            // Wait for 1 second
            await UniTask.WaitForSeconds(1f);

            // Revoke subscription
            await Asset0.RevokeSubscription(subscriberId, new EthereumAddress(Account5Address));

            // Try to subscribe and Assert that it throws OnlyUnrevokedUnauthorizedSubscriberError
            await OpenCreatorRailsService.Instance.Connect(5);
            try
            {
                await Asset0.Subscribe(subscriberId, new BigInteger(1));
                Assert.Fail();
            }
            catch (SmartContractCustomErrorRevertException e)
            {
                bool isError = Asset0.Service.FindCustomErrorException(e).ErrorABI.IsErrorABIForErrorType<OnlyUnrevokedUnauthorizedSubscriberError>();
                Assert.IsTrue(isError);
            }
        }
        
        [Test]
        public async Task Test_RevokeSubscription_TryCancel()
        {
            // Subscribe for 1 period
            string subscriberId = NewSubscriberId();
            await Asset0.Subscribe(subscriberId, new BigInteger(1));

            // Switch/Connect to Asset Owner
            await OpenCreatorRailsService.Instance.Connect(1);

            // Wait for 1 second
            await UniTask.WaitForSeconds(1f);

            // Revoke subscription
            await Asset0.RevokeSubscription(subscriberId, new EthereumAddress(Account5Address));

            // Try to cancel subscription and Assert that it throws OnlyUnrevokedUnauthorizedSubscriberError
            await OpenCreatorRailsService.Instance.Connect(5);
            try
            {
                await Asset0.CancelSubscription(subscriberId);
                Assert.Fail();
            }
            catch (SmartContractCustomErrorRevertException e)
            {
                bool isError = Asset0.Service.FindCustomErrorException(e).ErrorABI.IsErrorABIForErrorType<OnlyUnrevokedUnauthorizedSubscriberError>();
                Assert.IsTrue(isError);
            }
        }
        
        [Test]
        public async Task Test_RevokeSubscription_NewExpiration()
        {
            // Subscribe for 1 period
            string subscriberId = NewSubscriberId();
            DateTime expiry = await Asset0.Subscribe(subscriberId, new BigInteger(1));

            // Switch/Connect to Asset Owner
            await OpenCreatorRailsService.Instance.Connect(1);

            // Wait for 1 second
            await UniTask.WaitForSeconds(1f);

            // Revoke subscription
            await Asset0.RevokeSubscription(subscriberId, new EthereumAddress(Account5Address));

            // Assert current expiry in Asset0.Subscriptions < subscribing expiry
            await OpenCreatorRailsService.Instance.Connect(5);
            string subscribersIdHash = _cachedSubscriber.ToSubscriberIdHash().ToHex(true);
            var subscription = Asset0.Subscriptions.First(s => s.SubscriberIdHash == subscribersIdHash);
            Assert.Less(subscription.EndTime, expiry);
        }
        
        [Test]
        public async Task Test_RevokeSubscription_NewNonce()
        {
            // Subscribe for 1 period
            string subscriberId = NewSubscriberId();
            await Asset0.Subscribe(subscriberId, new BigInteger(1));

            // Connect to Asset Owner
            await OpenCreatorRailsService.Instance.Connect(1);
            // Set New (Random) Subscription Price
            await Asset0.SetSubscriptionPrice(new BigInteger(new Random().Next(1, 101) * 100));

            // Subscribe again for 1 Period (new nonce)
            await OpenCreatorRailsService.Instance.Connect(5);
            await Asset0.Subscribe(subscriberId, new BigInteger(1));

            // Connect to Asset Owner
            await OpenCreatorRailsService.Instance.Connect(1);

            // Wait for 1 second
            await UniTask.WaitForSeconds(1f);

            // Revoke Subscription
            await Asset0.RevokeSubscription(subscriberId, new EthereumAddress(Account5Address));

            // Assert that there's only one subscription matching this subscriber in Asset0.Subscriptions and its Nonce == 0
            await OpenCreatorRailsService.Instance.Connect(5);
            string subscribersIdHash = _cachedSubscriber.ToSubscriberIdHash().ToHex(true);
            var subscriptions = Asset0.Subscriptions.Where(s => s.SubscriberIdHash == subscribersIdHash).ToList();
            Assert.AreEqual(1, subscriptions.Count);
            Assert.AreEqual(BigInteger.Zero, subscriptions.First().Nonce);
        }
        
        [Test]
        public async Task Test_RevokeSubscription_Refund()
        {
            // Get balance of token in connected account
            BigInteger balanceBefore = await Asset0.PermitService.BalanceOfQueryAsync(Account5Address);

            // Subscribe for 3 periods
            string subscriberId = NewSubscriberId();
            await Asset0.Subscribe(subscriberId, new BigInteger(3));

            // Get balance of token in connected account again
            BigInteger balanceAfterSubscribe = await Asset0.PermitService.BalanceOfQueryAsync(Account5Address);

            // Assert 3x subscription price was deducted
            Assert.AreEqual(balanceBefore - balanceAfterSubscribe, Asset0.SubscriptionPrice * 3);

            // Wait for 1 second
            await UniTask.WaitForSeconds(1f);

            // Revoke Subscription (owner revokes on behalf of subscriber)
            await OpenCreatorRailsService.Instance.Connect(1);
            await Asset0.RevokeSubscription(subscriberId, new EthereumAddress(Account5Address));

            // Get balance of token in connected account again (query while connected as account 4)
            await OpenCreatorRailsService.Instance.Connect(5);
            BigInteger balanceAfterRevoke = await Asset0.PermitService.BalanceOfQueryAsync(Account5Address);

            // Check if > 2x subscription price was refunded (revoke refunds remaining time including partial period)
            Assert.Greater(balanceAfterRevoke - balanceAfterSubscribe, Asset0.SubscriptionPrice * 2);
        }
        
        [Test]
        public async Task Test_UnrevokeSubscription_IsRevoked()
        {
            // Subscribe for 1 period
            string subscriberId = NewSubscriberId();
            await Asset0.Subscribe(subscriberId, new BigInteger(1));

            // Assert isRevoked is false
            Assert.IsFalse(await Asset0.IsSubscriberRevoked(subscriberId));

            // Switch/Connect to Asset Owner
            await OpenCreatorRailsService.Instance.Connect(1);

            // Wait for 1 second
            await UniTask.WaitForSeconds(1f);

            // Revoke subscription
            await Asset0.RevokeSubscription(subscriberId, new EthereumAddress(Account5Address));

            // Assert isRevoked is true (connected as account 4 for correct hash)
            await OpenCreatorRailsService.Instance.Connect(5);
            Assert.IsTrue(await Asset0.IsSubscriberRevoked(subscriberId));

            // UnrevokeSubscription
            await OpenCreatorRailsService.Instance.Connect(1);
            await Asset0.UnrevokeSubscription(subscriberId, new EthereumAddress(Account5Address));

            // Assert isRevoked is false again (connected as account 4 for correct hash)
            await OpenCreatorRailsService.Instance.Connect(5);
            Assert.IsFalse(await Asset0.IsSubscriberRevoked(subscriberId));
        }
        
        [Test]
        public async Task Test_UnrevokeSubscription_IsActive()
        {
            // Subscribe for 1 period
            string subscriberId = NewSubscriberId();
            await Asset0.Subscribe(subscriberId, new BigInteger(1));

            // Assert isActive is true
            Assert.IsTrue(await Asset0.IsSubscriptionActive(subscriberId));

            // Switch/Connect to Asset Owner
            await OpenCreatorRailsService.Instance.Connect(1);

            // Wait for 1 second
            await UniTask.WaitForSeconds(1f);

            // Revoke subscription
            await Asset0.RevokeSubscription(subscriberId, new EthereumAddress(Account5Address));

            // Assert isActive is false (connected as account 4 for correct hash)
            await OpenCreatorRailsService.Instance.Connect(5);
            Assert.IsFalse(await Asset0.IsSubscriptionActive(subscriberId));

            // UnrevokeSubscription
            await OpenCreatorRailsService.Instance.Connect(1);
            await Asset0.UnrevokeSubscription(subscriberId, new EthereumAddress(Account5Address));

            // Assert isActive is false again since revoke expired it (connected as account 4 for correct hash)
            await OpenCreatorRailsService.Instance.Connect(5);
            Assert.IsFalse(await Asset0.IsSubscriptionActive(subscriberId));
        }
        
        [Test]
        public async Task Test_UnrevokeSubscription_TrySubscribe()
        {
            // Subscribe for 1 period
            string subscriberId = NewSubscriberId();
            await Asset0.Subscribe(subscriberId, new BigInteger(1));

            // Switch/Connect to Asset Owner
            await OpenCreatorRailsService.Instance.Connect(1);

            // Wait for 1 second
            await UniTask.WaitForSeconds(1f);

            // Revoke subscription
            await Asset0.RevokeSubscription(subscriberId, new EthereumAddress(Account5Address));

            // Assert isRevoked is true (connected as account 4 for correct hash)
            await OpenCreatorRailsService.Instance.Connect(5);
            Assert.IsTrue(await Asset0.IsSubscriberRevoked(subscriberId));

            // UnrevokeSubscription
            await OpenCreatorRailsService.Instance.Connect(1);
            await Asset0.UnrevokeSubscription(subscriberId, new EthereumAddress(Account5Address));

            // Assert Subscribing again doesn't throw
            await OpenCreatorRailsService.Instance.Connect(5);
            await Asset0.Subscribe(subscriberId, new BigInteger(1));
            Assert.Pass();
        }
        
        [Test]
        public async Task Test_Refresh_UpdatesCachedState()
        {
            // Asset0.SubscriptionPrice is populated from the indexer via InitializeAsync → Refresh().
            // We change the price on-chain directly through the low-level service (bypassing
            // IAsset.SetSubscriptionPrice so the in-memory cache is NOT updated), then call
            // Refresh() and assert the cache reflects the new on-chain value.
            BigInteger originalPrice = Asset0.SubscriptionPrice;
            BigInteger newPrice = new BigInteger(200);

            // Set new price as owner via the low-level service (skips in-memory cache update).
            await OpenCreatorRailsService.Instance.Connect(1);
            await Asset0.Service.SetSubscriptionPriceRequestAndWaitForReceiptAsync(newPrice);

            // The in-memory cache must still show the old price at this point.
            Assert.AreEqual(originalPrice, Asset0.SubscriptionPrice,
                "Cache must not change before Refresh() is called.");

            // The indexer refreshes until the asset is indexed or 10 seconds elapse ---
            // A fixed wait is fragile when the full test suite runs: the indexer may be
            // processing a backlog of blocks from earlier tests.
            int attempts = 10;
            
            for (int i = 0; i < attempts; i++)
            {
                // wait for 1 second
                await UniTask.WaitForSeconds(1f);
                // Refresh re-queries the indexer and overwrites all cached properties.
                await Asset0.Refresh();

                if (Asset0.SubscriptionPrice != originalPrice)
                {
                    break;
                }
            }

            Assert.AreEqual(newPrice, Asset0.SubscriptionPrice,
                "Asset.SubscriptionPrice must reflect the updated on-chain value after Refresh().");
        }

        [Test]
        public async Task Test_UnrevokeSubscription_TryCancel()
        {
            // Subscribe for 1 period
            string subscriberId = NewSubscriberId();
            await Asset0.Subscribe(subscriberId, new BigInteger(1));

            // Switch/Connect to Asset Owner
            await OpenCreatorRailsService.Instance.Connect(1);

            // Wait for 1 second
            await UniTask.WaitForSeconds(1f);

            // Revoke subscription
            await Asset0.RevokeSubscription(subscriberId, new EthereumAddress(Account5Address));

            // Assert isRevoked is true (connected as account 4 for correct hash)
            await OpenCreatorRailsService.Instance.Connect(5);
            Assert.IsTrue(await Asset0.IsSubscriberRevoked(subscriberId));

            // UnrevokeSubscription
            await OpenCreatorRailsService.Instance.Connect(1);
            await Asset0.UnrevokeSubscription(subscriberId, new EthereumAddress(Account5Address));

            // Assert Cancelling subscription doesn't throw
            await OpenCreatorRailsService.Instance.Connect(5);
            await Asset0.CancelSubscription(subscriberId);
            Assert.Pass();
        }
    }
}
