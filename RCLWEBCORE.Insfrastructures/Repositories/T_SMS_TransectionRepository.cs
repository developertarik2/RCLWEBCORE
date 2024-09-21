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
    public class T_SMS_TransectionRepository : Repository<ApplicationDbContext, T_SMS_TRANSECTION>, IT_SMS_TransectionRepository
    {
        private readonly ApplicationDbContext _db;
        public T_SMS_TransectionRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
    }
}
