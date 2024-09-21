using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEB.DATA.ViewModels
{
    public class LedgerVM
    {
        public ClientDetailsVM ClientDetails { get; set; }
        public List<LedgerDetailsVM> LedgerDetails { get; set;}
        public string? Code { get; set; }

        [DataType(DataType.Date)]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:/dd/MM/yyyy}")]
        public DateTime FromDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime ToDate { get; set; }
    }
}
