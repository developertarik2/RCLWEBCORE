using Microsoft.EntityFrameworkCore;
using RCLWEB.DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEBCORE.Insfrastructures.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
               : base(options)
        {
        }
        public DbSet<T_USER_LOGIN> AppUsers { get; set; }

        public DbSet<T_CLIENT1> T_CLIENTs { get; set; }
        public DbSet<T_SMS_TRANSECTION> T_SMS_TRANSECTIONs { get; set; }
        //public DbSet<T_CDBL_CHARGE> T_CDBL_CHARGEs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        }
    }
}
