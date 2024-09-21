using Microsoft.EntityFrameworkCore;
using RCLWEBCORE.Insfrastructures.Data;
using RCLWEBCORE.Insfrastructures.InterfaceRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEBCORE.Insfrastructures.Repositories
{
    public class UnitOfWork : IUnitOfWork /*where TContext : DbContext*/
    {
        private readonly ApplicationDbContext _db;
        private readonly SISRoyalDbContext _sisDb;

        //private readonly TContext _db;
        public UnitOfWork(ApplicationDbContext db, SISRoyalDbContext sisDb)
        {
            _db = db;
            UserLogin = new T_UserRepository(_db);
            T_Client = new T_ClientRepository(_db);
            T_SMS_Transection=new T_SMS_TransectionRepository(_db);
            T_CDBL_Charge = new T_CDBL_ChargeRepository(_db);

            _sisDb = sisDb;
            SP_Call = new SP_Call(_db,sisDb);
        }

        public IT_UserRepository UserLogin { get; private set; }

        public IT_ClientRepository T_Client { get; private set; }
        public IT_SMS_TransectionRepository T_SMS_Transection { get; private set; }
        public IT_CDBL_ChargeRepository T_CDBL_Charge { get; private set; }
        public ISP_Call SP_Call { get; private set; }

        public void Dispose()
        {
            _db.Dispose();
        }

        public void Save()
        {
            //_db.SaveChanges();
            if (_db.ChangeTracker.HasChanges())
                _db.SaveChanges();
        }
    }
}
