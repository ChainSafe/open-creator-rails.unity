using System;
using System.Numerics;
using Cysharp.Threading.Tasks;
using Io.ChainSafe.OpenCreatorRails.Contracts.Asset.ContractDefinition;

namespace Io.ChainSafe.OpenCreatorRails.Utils
{
    public static class SubscriptionTemplates
    {
        /// <summary>
        /// Subscribes for exactly one period of the given duration in seconds. The duration must
        /// be a whole divisor of <see cref="IAsset.SubscriptionDuration"/>; throws
        /// <see cref="InvalidOperationException"/> wrapping <c>InvalidSubscriptionDurationError</c>
        /// if it is not.
        /// </summary>
        /// <param name="asset">The asset to subscribe to.</param>
        /// <param name="duration">Desired subscription duration in seconds. Must evenly divide <see cref="IAsset.SubscriptionDuration"/>.</param>
        /// <param name="subscriberId">Plain-text subscriber identity string. The SDK derives the on-chain hash as <c>keccak256(abi.encode(subscriberId, connectedAccount))</c>.</param>
        /// <returns>The new subscription expiry as a local <see cref="DateTime"/>.</returns>
        public static UniTask<DateTime> SubscribeFor(this IAsset asset, double duration, string subscriberId)
        {
            double subscriptionDuration = asset.SubscriptionDuration.TotalSeconds;

            if (duration > subscriptionDuration || subscriptionDuration % duration != 0)
            {
                throw new InvalidOperationException(nameof(InvalidSubscriptionDurationError));
            }

            return asset.Subscribe(subscriberId, new BigInteger(subscriptionDuration / duration));
        }

        /// <summary>
        /// Subscribes for one day. Requires <see cref="IAsset.SubscriptionDuration"/>
        /// to be evenly divisible by one day.
        /// </summary>
        /// <param name="asset">The asset to subscribe to.</param>
        /// <param name="subscriberId">Plain-text subscriber identity string.</param>
        /// <returns>The new subscription expiry as a local <see cref="DateTime"/>.</returns>
        public static UniTask<DateTime> SubscribeDaily(this IAsset asset, string subscriberId)
        {
            double duration = TimeSpan.FromDays(1).TotalSeconds;

            return SubscribeFor(asset, duration, subscriberId);
        }

        /// <summary>
        /// Subscribes for one week. Requires <see cref="IAsset.SubscriptionDuration"/>
        /// to be evenly divisible by one week.
        /// </summary>
        /// <param name="asset">The asset to subscribe to.</param>
        /// <param name="subscriberId">Plain-text subscriber identity string.</param>
        /// <returns>The new subscription expiry as a local <see cref="DateTime"/>.</returns>
        public static UniTask<DateTime> SubscribeWeekly(this IAsset asset, string subscriberId)
        {
            double duration = TimeSpan.FromDays(7).TotalSeconds;

            return SubscribeFor(asset, duration, subscriberId);
        }

        /// <summary>
        /// Subscribes for 30 days. Requires <see cref="IAsset.SubscriptionDuration"/>
        /// to be evenly divisible by 30 days.
        /// </summary>
        /// <param name="asset">The asset to subscribe to.</param>
        /// <param name="subscriberId">Plain-text subscriber identity string.</param>
        /// <returns>The new subscription expiry as a local <see cref="DateTime"/>.</returns>
        public static UniTask<DateTime> SubscribeMonthly(this IAsset asset, string subscriberId)
        {
            double duration = TimeSpan.FromDays(30).TotalSeconds;

            return SubscribeFor(asset, duration, subscriberId);
        }

        /// <summary>
        /// Subscribes for 365 days. Requires <see cref="IAsset.SubscriptionDuration"/>
        /// to be evenly divisible by 365 days.
        /// </summary>
        /// <param name="asset">The asset to subscribe to.</param>
        /// <param name="subscriberId">Plain-text subscriber identity string.</param>
        /// <returns>The new subscription expiry as a local <see cref="DateTime"/>.</returns>
        public static UniTask<DateTime> SubscribeAnnually(this IAsset asset, string subscriberId)
        {
            double duration = TimeSpan.FromDays(365).TotalSeconds;

            return SubscribeFor(asset, duration, subscriberId);
        }
    }
}