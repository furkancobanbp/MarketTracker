using MarketTracker.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketTracker.Functions
{
    public class boolingerItems
    {
        public clsBoolingerBands boolinger(List<clsLastData> data)
        {
            clsBoolingerBands boolData = new clsBoolingerBands();

            boolData.averagePrice = data.Select(x => x.sonFiyat).Average();

            double sumOfSquareDiff = data.Select(x => (x.sonFiyat - boolData.averagePrice) * (x.sonFiyat - boolData.averagePrice)).Sum();

            double standardDeviation = Math.Sqrt(sumOfSquareDiff / data.Select(x => x.sonFiyat).Count());

            boolData.upperBand = boolData.averagePrice + (2 * standardDeviation);
            boolData.lowerBand = boolData.averagePrice - (2 * standardDeviation);

            return boolData;
        }
    }
}
