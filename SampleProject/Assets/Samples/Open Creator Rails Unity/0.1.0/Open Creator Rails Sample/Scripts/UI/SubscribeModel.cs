using Unity.Properties;

namespace Io.ChainSafe.OpenCreatorRails.Samples
{
    public struct SubscribeModel : IModel
    {
        [CreateProperty]
        public (string price, string duration)[] Prices { get; set; }
    }
}