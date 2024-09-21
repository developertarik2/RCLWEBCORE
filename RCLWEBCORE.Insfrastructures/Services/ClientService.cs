using RCLWEB.DATA.ViewModels;
using RCLWEBCORE.Insfrastructures.InterfaceRepo;
using RCLWEBCORE.Insfrastructures.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEBCORE.Insfrastructures.Services
{
    public class ClientService : IClientService
    {
        private readonly IUnitOfWork _unitofWork;

        public ClientService(IUnitOfWork unitofWork)
        {
            _unitofWork = unitofWork;
        }
        public ClientDetailsVM GetClientDetails(string code)
        {
            string query2 = @"SELECT acode,aname,boid,(addr1+' '+addr2+' ' +city) address,faname,moname,jname1,aatype FROM T_CLIENT WHERE acode='" + code + "'";
            var client = _unitofWork.SP_Call.ListByRawQueryBySis<ClientDetailsVM>(query2).AsQueryable().FirstOrDefault();
            return client;
            //if (client == null)
            //{
            //    return null;
            //}
            //throw new NotImplementedException();
        }
    }
}
