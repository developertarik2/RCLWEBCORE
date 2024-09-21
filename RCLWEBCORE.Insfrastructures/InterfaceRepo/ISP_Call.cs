using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEBCORE.Insfrastructures.InterfaceRepo
{
    public interface ISP_Call : IDisposable
    {
        IEnumerable<T> ReturnList<T>(string procedureName, DynamicParameters? param = null);

        IEnumerable<T> ReturnListFromSis<T>(string procedureName, DynamicParameters? param = null);

        //T ReturnSingle<T>(string procedureName, DynamicParameters param = null);

        void ExecuteWithoutReturn(string procedureName, DynamicParameters? param = null);
        void ExecuteWithoutReturnByQuery(string query);
        int ExecuteWithoutReturnByQuery2(string query);
        void ExecuteWithoutReturnByQuerySis(string query);
        T ExecuteReturnScaler<T>(string procedureName, DynamicParameters? param = null);

        List<T> ListByRawQuery<T>(string sql);

        List<T> ListByRawQueryBySis<T>(string sql);
    }
}
