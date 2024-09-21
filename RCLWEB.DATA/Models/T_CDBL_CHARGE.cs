using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEB.DATA.Models
{
    [Table("T_CDBL_CHARGE")]
    public class T_CDBL_CHARGE
    {
        public decimal? ROW_ID { get; set; }

        public string MR_NO { get; set; }

        public string RCODE { get; set; }

        public int YEAR { get; set; }

        public string FISCAL { get; set; }

        public double AMOUNT { get; set; }

        public int BRANCHCODE { get; set; }

        public DateTime DATE { get; set; }

        public string NOTE { get; set; }

        public string fis { get; set; }

        public string tamnt { get; set; }

        public string NAME { get; set; }

        public string transaction_id { get; set; }
    }
}
