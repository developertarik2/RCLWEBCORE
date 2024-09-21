using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEB.DATA.ViewModels
{
    public class IpoDetailsVM
    {
        public string IPO_CNAME { get; set; }

        public string IPO_NAME { get; set; }

        public decimal LOT_SIZE { get; set; }

        public decimal PRICE { get; set; }

        public DateTime START_DATE { get; set; }

        public DateTime END_DATE { get; set; }
        public decimal? T_PRICE { get; set; }
        public bool? ON_OFF { get; set; }

        public bool Applied { get; set; }
    }
}
