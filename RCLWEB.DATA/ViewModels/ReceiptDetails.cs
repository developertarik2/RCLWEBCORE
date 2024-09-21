using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEB.DATA.ViewModels
{
    public class ReceiptDetails
    {
        public DateTime? Date { get; set; }
        public string? VoucherNo { get; set; }
        public decimal? Deposit { get; set; }
        public decimal? Withdraw { get; set; }
    }
}
