using JetBrains.Annotations;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Io.ChainSafe.OpenCreatorRails.Samples
{
    public struct LandingModel : IModel
    {
        [CreateProperty]
        public StyleEnum<DisplayStyle> Connected =>
            OpenCreatorRailsService.Instance.Connected ? DisplayStyle.Flex : DisplayStyle.None;
        
        [CreateProperty]
        public StyleEnum<DisplayStyle> Disconnected =>
            OpenCreatorRailsService.Instance.Connected ? DisplayStyle.None : DisplayStyle.Flex;
        
        [CreateProperty]
        public StyleEnum<DisplayStyle> Paused =>
            Player.Instance != null ? (Player.Instance.Paused ? DisplayStyle.Flex : DisplayStyle.None) : DisplayStyle.None;
    }
}