using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEB.DATA.ViewModels
{
    public class RequisitionVM
    {
        public ClientDetailsVM ClientDetails { get; set; }

        public List<RequisitionReceiveVM> RequisitionReceive { get; set; }
        public List<RequisitionApproveVM> RequisitionApprove { get; set; }
        
    }
}
