using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

namespace Io.ChainSafe.OpenCreatorRails.Samples
{
    public struct SubscribeModel : IModel
    {
        [CreateProperty] public List<(string price, string duration)> Prices { get; }

        public IAsset[] Assets { get; }
        
        public SubscribeModel(IAsset[] assets)
        {
            Assets = assets;
            
            Prices = new List<(string price, string duration)>(Assets.Length);
            
            int index = 0;
            
            foreach (IAsset asset in Assets)
            {
                string price = $"{asset.SubscriptionPricePrimaryUnit} {asset.TokenSymbol} /";

                string duration = string.Empty;
                
                switch (index)
                {
                    case 0:
                        duration = "month";
                        break;
                    case 1:
                        duration = "year";
                        break;
                }

                Prices.Add((price, duration));
                
                index++;
            }
        }
    }
}