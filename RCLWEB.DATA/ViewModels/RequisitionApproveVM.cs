using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEB.DATA.ViewModels
{
    public class RequisitionApproveVM
    {
        public DateTime Date { get; set; }
        public string Code { get; set; }
        public decimal Amount { get; set; }
        public bool Posted { get; set; }
        public bool Approved { get; set; }
        public bool Rejected { get; set; }
    }
}
