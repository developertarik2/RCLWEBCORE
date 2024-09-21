using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEB.DATA.Models
{
    [Table("T_CLIENT1")]
    public class T_CLIENT1
    {
        [Key]
        public string? RCODE { get; set; }
        public decimal? Slno { get; set; }

        public string? Boid { get; set; }

        public string? Botype { get; set; }

        public string? Bocat { get; set; }

        

        public string? FhName { get; set; }

        public string? ShName { get; set; }

        public string? ThName { get; set; }

        public string? CpName { get; set; }

        public string? Sex { get; set; }

        public string? Dob { get; set; }

        public string? RegNum { get; set; }

        public string? Father { get; set; }

        public string? Mother { get; set; }

        public string? Occupation { get; set; }

        public string? Residency { get; set; }

        public string? Nationality { get; set; }

        public string? Add1 { get; set; }

        public string? add2 { get; set; }

        public string? add3 { get; set; }

        public string? city { get; set; }

        public string? state { get; set; }

        public string? country { get; set; }

        public string? pCode { get; set; }

        public string? mobile { get; set; }

        public string? mobile1 { get; set; }

        public string? email { get; set; }

        public string? fax { get; set; }

        public string? scc { get; set; }

        public string? fhShort { get; set; }

        public string? shShort { get; set; }

        public string? thShort { get; set; }

        public string? passNum { get; set; }

        public string? passIDate { get; set; }

        public string? passExDate { get; set; }

        public string? passIPlace { get; set; }

        public string? bName { get; set; }

        public string? bBranch { get; set; }

        public string? bAccNum { get; set; }

        public string? eftStatus { get; set; }

        public string? taxStatus { get; set; }

        public string? tin { get; set; }

        public string? eID { get; set; }

        public string? tID { get; set; }

        public string? routing { get; set; }

        public string? bCode { get; set; }

        public string? IntBAcctNum { get; set; }

        public string? Swift { get; set; }

        public string? SetupDate { get; set; }

        public string? boStatus { get; set; }

        public string? closeDate { get; set; }

        public DateTime? upFileDate { get; set; }

        public DateTime? lastUpdate { get; set; }

        public string? NID1 { get; set; }

        public string? NID2 { get; set; }

        public string? NID3 { get; set; }

    }
}
