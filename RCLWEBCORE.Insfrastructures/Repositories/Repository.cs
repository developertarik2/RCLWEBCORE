using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using RCLWEBCORE.Insfrastructures.InterfaceRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace RCLWEBCORE.Insfrastructures.Repositories
{
    public class Repository<TContext,T> : IRepository<T> where T : class where TContext : DbContext
    {
        //public readonly DbContext Context;
        //internal DbSet<T> dbSet;
        //public Repository(DbContext context)
        //{
        //    Context = context;
        //    this.dbSet = context.Set<T>();
        //}
        protected TContext Context;
        internal DbSet<T> dbSet;
        public Repository(TContext context)
        {
            Context = context;
            this.dbSet = context.Set<T>();
        }
        //=> Context = context;
        public void Add(T entity)
        {
            dbSet.Add(entity);
        }
        public IEnumerable<T> GetAll(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includeProperties = null)
        {
            IQueryable<T> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }
            //include properties will be comma seperated
            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            if (orderBy != null)
            {
                return orderBy(query).ToList();
            }
            return query.ToList();
        }
        public IQueryable<T> GetAllQ(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includeProperties = null)
        {
            IQueryable<T> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }
            //include properties will be comma seperated
            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            if (orderBy != null)
            {
                return orderBy(query);
            }
            return query;
        }
        public T GetFirstOrDefault(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includeProperties = null)
        {
            IQueryable<T> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            //include properties will be comma separeted
            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            if (orderBy != null)
            {
                return orderBy(query).FirstOrDefault();
            }

            return query.FirstOrDefault();
        }

        public T Get(int id)
        {
            return dbSet.Find(id);
        }

        public void Remove(int id)
        {
            T entityToRemove = dbSet.Find(id);
            Remove(entityToRemove);
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            dbSet.RemoveRange(entities);
        }

        public TResult GetFirstOrDefault<TResult>(Expression<Func<T, TResult>> selector,
                                          Expression<Func<T, bool>> predicate = null,
                                          Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
                                          Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
                                          bool disableTracking = true)
        {
            IQueryable<T> query = dbSet;
            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (orderBy != null)
            {
                return orderBy(query).Select(selector).FirstOrDefault();
            }
            else
            {
                return query.Select(selector).FirstOrDefault();
            }
        }
        /// <summary>
        /// Gets the first or default entity based on a predicate, orderby delegate and include delegate. This method default no-tracking query.
        /// </summary>
        /// <param name="selector">The selector for projection.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="orderBy">A function to order elements.</param>
        /// <param name="include">A function to include navigation properties</param>
        /// <param name="disableTracking"><c>True</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
        /// <returns>An <see cref="IPagedList{TEntity}"/> that contains elements that satisfy the condition specified by <paramref name="predicate"/>.</returns>
        /// <remarks>This method default no-tracking query.</remarks>
        public IEnumerable<T> GetAllThenInclude(/*Expression<Func<T, TResult>> selector,*/
                                         Expression<Func<T, bool>> predicate = null,
                                         Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
                                         Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
                                         bool disableTracking = true)
        {
            IQueryable<T> query = dbSet;
            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            if (orderBy != null)
            {
                return orderBy(query);
            }
            return query;
            //if (orderBy != null)
            //{
            //    return orderBy(query).Select(selector);
            //}
            //else
            //{
            //    return query.Select(selector);
            //}
        }

        public void Update(T entity)
        {

            dbSet.Attach(entity);
            Context.Entry(entity).State = EntityState.Modified;

            //  _context.Entry(entity).Property("SL").IsModified = false;

            //_context.Entry(entity).Property(x => x.a).IsModified = false;
        }

        public void UpdateExclude(T entity, string includeProperties = null, string excludeProperties = null)
        {

            dbSet.Attach(entity);
            Context.Entry(entity).State = EntityState.Modified;

            if (excludeProperties != null)
            {
                foreach (var excludeProperty in excludeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    // entry.Property(name).IsModified = false;
                    Context.Entry(entity).Property(excludeProperty).IsModified = false;
                    // query = query.Include(includeProperty);
                }
            }

            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    // entry.Property(name).IsModified = false;
                    //foreach (var name in _context.Entry(entity).CurrentValues.Properties.Except(entity.Equals))
                    Context.Entry(entity).Property(includeProperty).IsModified = true;
                    // query = query.Include(includeProperty);
                }
            }

            //foreach (var name in _context.Entry(entity).CurrentValues.Properties.Except())
            //{
            //    _context.Entry(entity).Property(name).IsModified = true;
            //}

            // _context.Entry(entity).Property("SL").IsModified = false;

            //_context.Entry(entity).Property(x => x.a).IsModified = false;
        }

        public void BulkInsert(IList<T> entities)
        {
            using (var transactionScope = new TransactionScope())
            {
                //using (var _context = new ApplicationDbContext())
                //{
                // some stuff in dbcontext
                //_context.BulkInsert(entities);
                //_context.SaveChanges();
                //ctx.Commit();
                transactionScope.Complete();
                //}
            }
        }

        public virtual void Update(T entity, params Expression<Func<T, object>>[] updatedProperties)
        {
            //Ensure only modified fields are updated.
            var dbEntityEntry = Context.Entry(entity);
            if (updatedProperties.Any())
            {
                //update explicitly mentioned properties
                foreach (var property in updatedProperties)
                {
                    dbEntityEntry.Property(property).IsModified = true;
                }
            }
            else
            {
                //no items mentioned, so find out the updated entries
                foreach (var property in dbEntityEntry.OriginalValues.Properties)
                {
                    var original = dbEntityEntry.OriginalValues.GetValue<object>(property);
                    var current = dbEntityEntry.CurrentValues.GetValue<object>(property);
                    if (original != null && !original.Equals(current))
                        dbEntityEntry.Property("").IsModified = true;
                }
            }
        }

        public int GetCount(Expression<Func<T, bool>> filter = null)
        {
            IQueryable<T> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }
            return query.ToList().Count;
            //throw new NotImplementedException();
        }
    }
}
