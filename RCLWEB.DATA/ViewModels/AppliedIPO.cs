using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEB.DATA.ViewModels
{
    public class AppliedIPO
    {
        public int SL { get; set; }
        public DateTime Date { get; set; }
        public string IPOName { get; set; }
        public string MRNo { get; set; }
        public string Status { get; set; }
        public decimal Investment { get; set; }
        public int Allotment { get; set; }
    }
}
