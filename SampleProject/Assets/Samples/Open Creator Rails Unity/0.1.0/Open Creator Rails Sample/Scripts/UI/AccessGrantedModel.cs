using System;
using Unity.Properties;

namespace Io.ChainSafe.OpenCreatorRails.Samples
{
    public struct AccessGrantedModel : IModel
    {
        [CreateProperty]
        public string Years { get; private set; }
        
        [CreateProperty]
        public string Months { get; private set; }
        
        [CreateProperty]
        public string Days { get; private set; }
        
        [CreateProperty]
        public string Hours { get; private set; }
        
        public AccessGrantedModel(DateTime endTime)
        {
            DateTime now = DateTime.Now;
            
            int years = endTime.Year - now.Year;
            
            int months = endTime.Month - now.Month;
            
            int days = endTime.Day - now.Day;
            
            int hours = endTime.Hour - now.Hour;
            
            // Adjust hours first
            if (hours < 0)
            {
                days--;
                hours += 24;
            }
            
            // Adjust days
            if (days < 0)
            {
                months--;
                days += DateTime.DaysInMonth(endTime.Year, endTime.Month == 1 ? 12 : endTime.Month - 1);
            }

            // Adjust months
            if (months < 0)
            {
                years--;
                months += 12;
            }

            Years = $"{years:D2}";
            Months = $"{months:D2}";
            Days = $"{days:D2}";
            Hours = $"{hours:D2}";
        }
    }
}