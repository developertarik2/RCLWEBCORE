using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEB.DATA.ViewModels
{
    public class LedgerDetailsVM
    {
        public string? Vno { get; set; }
        public string? Tdate { get; set; }
        public string? Type { get; set; }
        public string? Narr { get; set; }
        public int Quantity { get; set; }
        public double? Rate { get; set; }
        public double? Debit { get; set; }
        public double? Credit { get; set; }
        public double? Commission { get; set; }
        public double? Balance { get; set; }

        public double? TotalBalance { get; set; }
    }
}
