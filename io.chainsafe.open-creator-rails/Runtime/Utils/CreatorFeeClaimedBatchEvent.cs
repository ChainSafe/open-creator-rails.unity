using Io.ChainSafe.OpenCreatorRails.Contracts.Asset.ContractDefinition;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Io.ChainSafe.OpenCreatorRails.Utils
{
    /// <summary>
    /// A substitute event class for <see cref="CreatorFeeClaimedBatchEventDTO"/> because NEthereum
    /// fails to decode <c>bytes32[] indexed subscribers</c> Solidity into the generated C# counterpart type <c>List~byte[]~</c>
    /// </summary>
    public class CreatorFeeClaimedBatchEvent : CreatorFeeClaimedBatchEventDTOBase
    {
        [Parameter("bytes32[]", "subscribers", 1, true )]
        public new string Subscribers { get; set; }
    }
}