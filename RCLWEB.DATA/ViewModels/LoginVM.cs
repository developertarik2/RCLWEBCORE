using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEB.DATA.ViewModels
{
    public class LoginVM
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [EnumDataType(typeof(UserType))]
        public UserType UserType { get; set; }

        public string? ReturnUrl { get; set; }
    }

    public enum UserType
    {
        User = 0,
        Branch = 1
    }
}
