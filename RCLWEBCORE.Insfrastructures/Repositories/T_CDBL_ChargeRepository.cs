using RCLWEB.DATA.Models;
using RCLWEBCORE.Insfrastructures.Data;
using RCLWEBCORE.Insfrastructures.InterfaceRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEBCORE.Insfrastructures.Repositories
{
    public class T_CDBL_ChargeRepository : Repository<ApplicationDbContext, T_CDBL_CHARGE>, IT_CDBL_ChargeRepository
    {
        private readonly ApplicationDbContext _db;
        public T_CDBL_ChargeRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
    }
}
