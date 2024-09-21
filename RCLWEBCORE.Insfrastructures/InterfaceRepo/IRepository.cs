using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RCLWEBCORE.Insfrastructures.InterfaceRepo
{
    public interface IRepository<T> where T : class
    {
        T Get(int id);

        IEnumerable<T> GetAll(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = null
                );

        IQueryable<T> GetAllQ(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = null
                );
        T GetFirstOrDefault(
            Expression<Func<T, bool>> filter = null,
               Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
              string includeProperties = null
            );
        IEnumerable<T> GetAllThenInclude(/*Expression<Func<T, TResult>> selector*/
                                         Expression<Func<T, bool>> predicate = null,
                                         Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
                                         Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
                                         bool disableTracking = true);

        int GetCount(Expression<Func<T, bool>> filter = null);
        void Add(T entity);

        void Update(T entity);
        void UpdateExclude(T entity, string includeProperties = null, string excludeProperties = null);

        void Remove(int id);
        void Remove(T entity);

        void RemoveRange(IEnumerable<T> entities);
    }
}
