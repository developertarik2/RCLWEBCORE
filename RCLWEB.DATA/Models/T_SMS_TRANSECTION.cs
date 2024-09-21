using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEB.DATA.Models
{
    [Table("T_SMS_TRANSECTION")]
    public class T_SMS_TRANSECTION
    {
        [Key]
        public decimal Sl { get; set; }

        public string SMS_Number { get; set; }

        public string RCODE { get; set; }

        public string Amount { get; set; }

        public DateTime? Dat { get; set; }

        public int? Flag1 { get; set; }

        public int? Flag2 { get; set; }

        public string Status { get; set; }

        public decimal? Clr { get; set; }

        public int? Download { get; set; }
    }
}
