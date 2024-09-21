using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEB.DATA.ViewModels
{
    public class TaxPartialVM
    {
        public ClientDetailsVM ClientDetails { get; set; }
        public TaxDateToDateVM TaxDateToDate { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
