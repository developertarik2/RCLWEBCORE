using Dapper;
using Microsoft.Data.SqlClient;
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
    public class SP_Call : ISP_Call
    {
        private readonly ApplicationDbContext _db;

        private readonly SISRoyalDbContext _sisDb;
        private static string ConnectionString = "";
        private static string ConnectionString2 = "";

        public SP_Call(ApplicationDbContext db,SISRoyalDbContext sisDb)
        {
            _db = db;
            _sisDb = sisDb;
            ConnectionString = db.Database.GetDbConnection().ConnectionString;
            ConnectionString2 = sisDb.Database.GetDbConnection().ConnectionString;
        }
        public void Dispose()
        {
            _db.Dispose();
        }

        public T ExecuteReturnScaler<T>(string procedureName, DynamicParameters? param = null)
        {
            using (SqlConnection sqlCon = new SqlConnection(ConnectionString))
            {
                sqlCon.Open();
                return (T)Convert.ChangeType(sqlCon.ExecuteScalar<T>(procedureName, param, commandType: System.Data.CommandType.StoredProcedure), typeof(T));
            }
        }

        public void ExecuteWithoutReturn(string procedureName, DynamicParameters? param = null)
        {
            using (SqlConnection sqlCon = new SqlConnection(ConnectionString))
            {
                sqlCon.Open();
                sqlCon.Execute(procedureName, param, commandType: System.Data.CommandType.StoredProcedure);
            }
        }

        public List<T> ListByRawQuery<T>(string sql)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                return connection.Query<T>(sql).ToList();
                //return data;
                // use data
            }
        }
        public List<T> ListByRawQueryBySis<T>(string sql)
        {
            using (var connection = new SqlConnection(ConnectionString2))
            {
                return connection.Query<T>(sql).ToList();
                //return data;
                // use data
            }
        }

        public IEnumerable<T> ReturnList<T>(string procedureName, DynamicParameters? param = null)
        {
            using (SqlConnection sqlCon = new SqlConnection(ConnectionString))
            {
                sqlCon.Open();
                return sqlCon.Query<T>(procedureName, param, commandType: System.Data.CommandType.StoredProcedure);
            }
        }

        public IEnumerable<T> ReturnListFromSis<T>(string procedureName, DynamicParameters? param = null)
        {
            using (SqlConnection sqlCon = new SqlConnection(ConnectionString2))
            {
                sqlCon.Open();
                return sqlCon.Query<T>(procedureName, param, commandType: System.Data.CommandType.StoredProcedure);
            }
        }

        public void ExecuteWithoutReturnByQuery(string query)
        {
            using (SqlConnection sqlCon = new SqlConnection(ConnectionString))
            {
                sqlCon.Open();
                sqlCon.Execute(query, commandType: System.Data.CommandType.Text);
            }
        }

        public int ExecuteWithoutReturnByQuery2(string query)
        {
            using (SqlConnection sqlCon = new SqlConnection(ConnectionString))
            {
                sqlCon.Open();
               return sqlCon.Execute(query, commandType: System.Data.CommandType.Text);
            }
        }
        public void ExecuteWithoutReturnByQuerySis(string query)
        {
            using (SqlConnection sqlCon = new SqlConnection(ConnectionString2))
            {
                sqlCon.Open();
                sqlCon.Execute(query, commandType: System.Data.CommandType.Text);
            }
        }
    }
}
