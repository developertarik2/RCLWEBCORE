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
    public class T_UserRepository : Repository<ApplicationDbContext, T_USER_LOGIN>, IT_UserRepository
    {
        private readonly ApplicationDbContext _db;
        public T_UserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
    }
}
