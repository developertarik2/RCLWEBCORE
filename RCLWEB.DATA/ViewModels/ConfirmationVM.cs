using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEB.DATA.ViewModels
{
    public class ConfirmationVM
    {
        public ConfirmationByDateVM ConfirmationByDateVM { get; set; }
        public ClientDetailsVM ClientDetails { get; set; }

        public string Code { get; set; }

        [DataType(DataType.Date)]
        public DateTime FromDate { get; set; }
    }
}
