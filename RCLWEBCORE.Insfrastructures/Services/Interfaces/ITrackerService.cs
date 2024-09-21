using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEBCORE.Insfrastructures.Services.Interfaces
{
    public interface ITrackerService
    {
        void Insert_To_Tracker(string action_type, string user_id, string? ip_address, int status = 1);
    }
}
