using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEB.DATA.ViewModels
{
    public class RequisitionReceiveVM
    {
        public string RCode { get; set; }
        public decimal Amount { get; set; }
        public DateTime Dat { get; set; }
        public int Flag1 { get; set; }
        public int Flag2 { get; set; }
        public string Status { get; set; }
        public int Clr { get; set; }


    }
}
