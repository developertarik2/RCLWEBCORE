using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEB.DATA.ViewModels
{
    public class ConfirmationDetailsVM
    {
        public string Exch { get; set; }
        public string CODE { get; set; }
        public string Instrument { get; set; }
        public string BuyQty { get; set; }
        public string BuyAmt { get; set; }
        public string BuyRate { get; set; }
        public string SaleQty { get; set; }
        public string SaleAmt { get; set; }
        public string SaleRate { get; set; }
        public string BalQty { get; set; }
        public string Com_B_S { get; set; }
        public string Balance { get; set; }
        //public string NAOT { get; set; }
    }
}
