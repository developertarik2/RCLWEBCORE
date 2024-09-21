using RCLWEB.DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEB.DATA.ViewModels
{
    public class CDBLChargeReportVM
    {
        public string RCODE { get; set; }
        public List<T_CDBL_CHARGE> CHARGEs { get; set; }
    }
}
