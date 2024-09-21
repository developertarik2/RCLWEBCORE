using RCLWEB.DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEB.DATA.ViewModels
{
    public class EftSubmitVM
    {
        public string RCODE { get; set; }
        public decimal? MatureBal { get; set; }
        public decimal? LedgrBal { get; set; }
        public int Amount { get; set; }

        public List<T_SMS_TRANSECTION>? PendingTrans { get; set; }
    }
}
