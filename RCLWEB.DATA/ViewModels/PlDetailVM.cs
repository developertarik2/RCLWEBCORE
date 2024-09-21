using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEB.DATA.ViewModels
{
    public class PlDetailVM
    {
        public ClientDetailsVM ClientDetails { get; set; }
        public PlDetailsPartialVM PlDetailsPartial { get; set; }
        public string Code { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
