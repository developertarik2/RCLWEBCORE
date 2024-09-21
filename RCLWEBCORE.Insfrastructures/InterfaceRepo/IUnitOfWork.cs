using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEBCORE.Insfrastructures.InterfaceRepo
{
    public interface IUnitOfWork : IDisposable
    {
        IT_UserRepository UserLogin { get; }
        IT_ClientRepository T_Client { get; }
        IT_SMS_TransectionRepository T_SMS_Transection { get; }
        IT_CDBL_ChargeRepository T_CDBL_Charge { get; }
        ISP_Call SP_Call { get; }
        void Save();
        //public void Dispose()
        //{
        //    throw new NotImplementedException();
        //}
    }
}
