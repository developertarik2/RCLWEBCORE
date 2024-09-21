using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEB.DATA.ViewModels
{
    public class LivePortfolioVM
    {
        public List<LivePortfolioCompanyVM> LivePortfolioLists { get; set; }
        public ClientDetailsVM ClientDetails { get; set; }
        public decimal? GainLossBalance { get; set; }
        public decimal? AccruedBal { get; set; }
        public decimal? MaturedBal { get; set; }
        public decimal? LedgerBal { get; set; }
        public string? SaleRec { get; set; }
        public decimal? TotalBuyCost { get; set; }
        public decimal? MarketVal { get; set; }
        public decimal? EquityBal { get; set; }
        public decimal? UnrealiseBal { get; set; }
        public decimal? TotalCapital { get; set; }
        public decimal? RglBal { get; set; }
        public double? ChargeFee { get; set; }
    }
}
