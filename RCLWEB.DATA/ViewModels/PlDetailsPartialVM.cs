using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEB.DATA.ViewModels
{
    public class PlDetailsPartialVM
    {
        public List<PlDetailList> PlDetailList { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal BoughtCost { get; set; }
        public decimal SoldCost { get; set; }
        public decimal RealisedGain_Loss { get; set; }
        public decimal BalanceAmnt { get; set; }
        public decimal MarketAmnt { get; set; }
        public decimal UnrealisedGain { get; set; }

        public List<IPOShareList> IPOShareLists { get; set; }
        public List<IPOShareList> BonusShareLists { get; set; }
        public List<IPOShareList> RightShareLists { get; set; }

        public double? OpeningShareBal { get; set; }
        public decimal? LedgerBal { get; set; }
        public decimal? PortfolioValueMarket { get; set; }
        public decimal? PortfolioValueCost { get; set; }
        public decimal? Deposit { get; set; }
        public decimal? WithdrawnAmount { get; set; }
        public decimal? Charges { get; set; }
        public decimal? NetDeposit { get; set; }
        public double? RealisedCapitalGainLoss { get; set; }
    }
}
