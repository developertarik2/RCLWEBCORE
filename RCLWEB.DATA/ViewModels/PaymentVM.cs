using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEB.DATA.ViewModels
{
    public class PaymentVM
    {
        public string Code { get; set; }
        [Required]
        public decimal Amount { get; set; }
    }
}
