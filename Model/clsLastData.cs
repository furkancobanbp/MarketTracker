using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketTracker.Model
{
    public class clsLastData
    {
        public string kontratAd { get; set; }
        public double bestBuyPrice { get; set; }
        public double bestSellPrice { get; set; }
        public int bestBuyQuantity { get; set; }
        public int bestSellQuantity { get; set; }
        public double minFiyat { get; set; }
        public double maxFiyat { get; set; }
        public double sonFiyat { get; set; }
        public int sonMiktar { get; set; }
        public int hacim { get; set; }
        public string trendImageName { get; set; }
        public int trendImageId { get; set; }
        public double ortalamaFiyat { get; set; }
        public double ptf { get; set; }
    }
}
