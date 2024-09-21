using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEB.DATA.Models
{
    [Table("T_IPO_Details")]
    public class T_IPO_Details
    {
        public decimal IPO_SL { get; set; }

        public string IPO_CNAME { get; set; }

        public string IPO_NAME { get; set; }

        public decimal LOT_SIZE { get; set; }

        public decimal PRICE { get; set; }

        public DateTime START_DATE { get; set; }

        public DateTime END_DATE { get; set; }

        public string DESIGNATION { get; set; }

        public string ADDRESS { get; set; }

        public string ADDITIONAL_TXT { get; set; }

        public decimal? S_CHARGE { get; set; }

        public decimal? T_PRICE { get; set; }

        public decimal? GT { get; set; }

        public bool? ON_OFF { get; set; }

        public string T_PRICE_WORD { get; set; }

        public string IPO_SHORT_CODE { get; set; }

        public decimal? INT_RATE { get; set; }

        public decimal? LOAN_RATIO { get; set; }

        public DateTime? LOAN_SETTL_DATE { get; set; }

    }
}
