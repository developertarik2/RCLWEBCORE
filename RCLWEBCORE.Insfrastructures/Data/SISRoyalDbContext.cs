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
    public class SISRoyalDbContext : DbContext
    {
        public SISRoyalDbContext(DbContextOptions<SISRoyalDbContext> options)
               : base(options)
        {
        }
        //public DbSet<T_USER_LOGIN> AppUsers { get; set; }

        //public DbSet<T_CLIENT1> T_CLIENTs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        }
    }
}
