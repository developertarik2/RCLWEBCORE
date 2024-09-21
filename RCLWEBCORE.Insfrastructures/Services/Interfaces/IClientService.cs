using RCLWEB.DATA.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEBCORE.Insfrastructures.Services.Interfaces
{
    public interface IClientService
    {
        ClientDetailsVM GetClientDetails(string code);
    }
}
