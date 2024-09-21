using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEB.DATA.ViewModels
{
    public class ConfirmationDeatils
    {
        public string? Cocd { get; set; }
        public string? Acode { get; set; }
        public string? Firmsnm1 { get; set; }
        public decimal? BuyQnty { get; set; }
        public decimal? Bamnt { get; set; }
        public decimal? BuyAmt { get; set; }
        //public decimal? BuyQty { get; set; }
        public decimal? SaleQnty { get; set; }
        public decimal? Samnt { get; set; }
        public decimal? SaleAmt { get; set; }
        //public decimal? SaleQty { get; set; }
        public decimal? BalQnty { get; set; }
        public decimal? Commsn { get; set; }
        public decimal? Balance { get; set; }
    }
}
