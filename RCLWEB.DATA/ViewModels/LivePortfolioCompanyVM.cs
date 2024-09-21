using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEB.DATA.ViewModels
{
    public class LivePortfolioCompanyVM:PortfolioCompanyVM
    {
        public double? MarketRate { get; set; }
        public double? MarketValue { get; set; }
        public double? GainLoss { get; set; }
    }
}
