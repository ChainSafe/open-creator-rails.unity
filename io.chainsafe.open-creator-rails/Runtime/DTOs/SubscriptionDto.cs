using System;
using System.Numerics;
using Io.ChainSafe.OpenCreatorRails.Utils;

namespace Io.ChainSafe.OpenCreatorRails.DTOs
{
    /// <summary>
    /// Represents a single subscription record for a subscriber on an asset. Records are
    /// kept in <see cref="IAsset.Subscriptions"/> and updated in response to contract events.
    /// A subscriber may have at most one active record at a time; a new nonce is created on
    /// renewal when the previous period expires or when subscription terms change.
    /// </summary>
    public struct SubscriptionDto
    {
        /// <summary>
        /// On-chain subscriber identity hash, computed as
        /// <c>keccak256(abi.encode(subscriberId, connectedAccount))</c>, where
        /// <c>subscriberId</c> is the plain-text identity string and <c>connectedAccount</c>
        /// is the wallet address used when the subscription was created.
        /// </summary>
        public string SubscriberIdHash { get; private set; }

        /// <summary>
        /// Monotonically increasing nonce for this subscriber and creates a new subscription record. Increments each time a new
        /// subscription record is created (i.e. on <c>SubscriptionAdded</c> or <c>SubscriptionRenewed</c>
        /// events). Extensions do not increment the nonce.
        /// </summary>
        public BigInteger Nonce { get; private set; }

        /// <summary>Start time of the current subscription record (local <see cref="DateTime"/>).</summary>
        public DateTime StartTime { get; private set; }

        /// <summary>
        /// Expiry time of the current subscription record (local <see cref="DateTime"/>).
        /// Updated on extension, Possibly shortened on cancellation or revocation.
        /// </summary>
        public DateTime EndTime { get; private set; }

        /// <summary>
        /// Per-period subscription price snapshot captured at the time this record was created,
        /// in the payment token's smallest unit. Used to compute pro-rata refunds.
        /// </summary>
        public BigInteger SubscriptionPrice { get; private set; }

        /// <summary>
        /// Registry fee share snapshot (0–100) captured at the time this record was created.
        /// Used together with <see cref="SubscriptionPrice"/> to compute creator and registry fee splits.
        /// </summary>
        public BigInteger RegistryFeeShare { get; private set; }

        /// <summary>
        /// Address of the wallet that paid for this subscription record.
        /// This address receives refunds if the subscription is cancelled or revoked.
        /// </summary>
        public EthereumAddress Payer { get; private set; }

        /// <summary><c>true</c> if <see cref="EndTime"/> is in the past; otherwise <c>false</c>.</summary>
        public bool IsExpired { get; private set; }

        /// <summary>
        /// <c>true</c> if the asset owner has permanently revoked this subscriber.
        /// A revoked subscriber cannot resubscribe or cancel subscriptions until the asset owner calls
        /// <see cref="IAsset.UnrevokeSubscription"/>.
        /// </summary>
        public bool IsRevoked { get; private set; }

        /// <summary><c>true</c> if the subscription is neither expired nor revoked; otherwise <c>false</c>.</summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Initializes a new <see cref="SubscriptionDto"/> with all fields.
        /// </summary>
        /// <param name="subscriberIdHash">On-chain subscriber identity hash: <c>keccak256(abi.encode(subscriberId, connectedAccount))</c>.</param>
        /// <param name="nonce">Current subscription nonce for this subscription record of the subscriber.</param>
        /// <param name="startTime">Start time of the subscription record.</param>
        /// <param name="endTime">Expiry time of the subscription record.</param>
        /// <param name="subscriptionPrice">Per-period price snapshot at creation time, in token smallest unit.</param>
        /// <param name="registryFeeShare">Registry fee share snapshot (0–100) at creation time.</param>
        /// <param name="payer">Address of the payer (refund beneficiary on cancel/revoke).</param>
        /// <param name="isExpired">Whether the subscription has expired.</param>
        /// <param name="isRevoked">Whether the subscriber has been permanently revoked.</param>
        /// <param name="isActive">Whether the subscription is currently active (not expired and not revoked).</param>
        public SubscriptionDto(string subscriberIdHash, BigInteger nonce, DateTime startTime, DateTime endTime,
            BigInteger subscriptionPrice, BigInteger registryFeeShare, EthereumAddress payer, bool isExpired, bool isRevoked, bool isActive)
        {
            SubscriberIdHash = subscriberIdHash;
            Nonce = nonce;
            StartTime = startTime;
            EndTime = endTime;
            SubscriptionPrice = subscriptionPrice;
            RegistryFeeShare = registryFeeShare;
            Payer = payer;
            IsExpired = isExpired;
            IsRevoked = isRevoked;
            IsActive = isActive;
        }

        /// <summary>
        /// Returns a copy of this record with subscription extended to <paramref name="endTime"/>
        /// if it is later than the current value.
        /// </summary>
        /// <param name="endTime">The new expiry date for this subscription.</param>
        /// <returns>The updated <see cref="SubscriptionDto"/>.</returns>
        public SubscriptionDto Extended(DateTime endTime)
        {
            if (EndTime < endTime)
            {
                EndTime = endTime;
                
                IsExpired = false;
                
                IsRevoked = false;
                
                IsActive = true;
            }

            return this;
        }

        /// <summary>
        /// Returns a copy of this record with <see cref="EndTime"/> shortened to
        /// <paramref name="endTime"/> if it is earlier than the current value.
        /// </summary>
        /// <param name="endTime">The new expiry date to apply for this subscription.</param>
        /// <returns>The updated <see cref="SubscriptionDto"/>.</returns>
        public SubscriptionDto Shortened(DateTime endTime)
        {
            if (endTime < EndTime)
            {
                EndTime = endTime;
            }

            return this;
        }

        /// <summary>
        /// Returns a copy of this record with <see cref="IsRevoked"/> set to <c>true</c>.
        /// </summary>
        /// <returns>The updated <see cref="SubscriptionDto"/>.</returns>
        public SubscriptionDto Revoked()
        {
            if (!IsRevoked)
            {
                IsRevoked = true;
            }

            return this;
        }

        /// <summary>
        /// Returns a copy of this record with <see cref="IsRevoked"/> set to <c>false</c>.
        /// </summary>
        /// <returns>The updated <see cref="SubscriptionDto"/>.</returns>
        public SubscriptionDto Unrevoked()
        {
            if (IsRevoked)
            {
                IsRevoked = false;
            }

            return this;
        }
    }
}
