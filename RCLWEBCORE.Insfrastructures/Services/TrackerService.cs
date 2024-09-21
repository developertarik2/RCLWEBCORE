using RCLWEBCORE.Insfrastructures.InterfaceRepo;
using RCLWEBCORE.Insfrastructures.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEBCORE.Insfrastructures.Services
{
    public class TrackerService:ITrackerService
    {
        private readonly IUnitOfWork _unitofWork;

        public TrackerService(IUnitOfWork unitofWork)
        {
            _unitofWork = unitofWork;
        }

        public void Insert_To_Tracker(string action_type, string user_id,string? ip_address, int status = 1)
        {
            string query = @"INSERT INTO T_WEB_USER_TRACKER (user_id, IP_address, action_type, status) 
             VALUES ('" + user_id + "', '" + ip_address + "', '" + action_type + "', "+status+")";
            _unitofWork.SP_Call.ExecuteWithoutReturnByQuery(query);
            //throw new NotImplementedException();
        }
    }
}
