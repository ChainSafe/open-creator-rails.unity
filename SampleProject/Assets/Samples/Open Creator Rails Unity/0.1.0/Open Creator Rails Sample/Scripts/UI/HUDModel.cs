using Unity.Properties;
using UnityEngine.UIElements;

namespace Io.ChainSafe.OpenCreatorRails.Samples
{
    public struct HUDModel : IModel
    {
        [CreateProperty]
        public StyleEnum<DisplayStyle> Connected =>
            OpenCreatorRailsService.Instance.Connected ? DisplayStyle.Flex : DisplayStyle.None;

        [CreateProperty]
        public StyleEnum<DisplayStyle> Disconnected =>
            OpenCreatorRailsService.Instance.Connected ? DisplayStyle.None : DisplayStyle.Flex;

        [CreateProperty]
        public string ConnectedAddress =>
            OpenCreatorRailsService.Instance.Connected
                ? OpenCreatorRailsService.Instance.WalletProvider.ConnectedAccount.Value
                : string.Empty;

        [CreateProperty]
        public StyleEnum<DisplayStyle> Interactable =>
            Player.Instance != null
                ? (Player.Instance.Interactable ? DisplayStyle.Flex : DisplayStyle.None)
                : DisplayStyle.None;
    }
}