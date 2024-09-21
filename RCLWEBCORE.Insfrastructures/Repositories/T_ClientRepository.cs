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
    public class T_ClientRepository : Repository<ApplicationDbContext, T_CLIENT1>, IT_ClientRepository
    {
        private readonly ApplicationDbContext _db;
        public T_ClientRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
    }
}
