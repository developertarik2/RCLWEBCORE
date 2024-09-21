using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEB.DATA.Models
{
    //[NotMapped]
    [Table("T_USER_LOGIN")]
    public class T_USER_LOGIN
    {
        [Key]
        public string Mobile { get; set; }

        public string Password { get; set; }

        public decimal? Lock { get; set; }

        public decimal? Flag { get; set; }

        public decimal Sl { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }

        public string? ProfilePic { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? AlternativePhone { get; set; }

        public string? AlternativeEmail { get; set; }

        public string? City { get; set; }

        public string? Country { get; set; }

        public string? Ukey { get; set; }
    }
}


