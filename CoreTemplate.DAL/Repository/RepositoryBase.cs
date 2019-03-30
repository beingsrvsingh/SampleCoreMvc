using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CoreTemplate.DAL.Models;

namespace CoreTemplate.DAL.Repository
{
    /// <summary>
    /// Repository base class
    /// </summary>
    /// <typeparam name="T">the type of parameter passed</typeparam>
    public class RepositoryBase<T> where T : class
    {
        /// <summary>
        /// Initializes a new instance of the RepositoryBase class.
        /// </summary>
        /// <param name="dbcontext">instance of HPAssistEntities class</param>
        internal RepositoryBase(CoreTemplateContext dbcontext)
        {
            this.dataContext = dbcontext;
            this.dbSet = dbcontext.Set<T>();
        }

        /// <summary>
        /// Gets or sets instance of HPAssistEntities 
        /// </summary>
        internal CoreTemplateContext dataContext { get; set; }

        /// <summary>
        /// Gets or sets instance of Database Set
        /// </summary>
        internal DbSet<T> dbSet { get; set; }

        /// <summary>
        /// This method is used to get list
        /// </summary>
        /// <remarks>Author: Dave Digvijay</remarks>
        /// <param name="filter">expression of filter query</param>
        /// <param name="orderBy">function for orderBy</param>
        /// <param name="includeProperties">string values of properties</param>
        /// <returns>Generic list</returns>
        public virtual List<T> Get(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "")
        {
            IQueryable<T> query = this.dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return orderBy(query).ToList();
            }
            else
            {
                return query.ToList();
            }
        }

        /// <summary>
        /// get generic method to order
        /// </summary>
        /// <remarks>Author: Henry Heeralal</remarks>
        /// <param name="orderColumn">column against ordering</param>
        /// <param name="orderType">ordering type</param>
        /// <returns>result after ordering</returns>
        public virtual Func<IQueryable<T>, IOrderedQueryable<T>> GetOrderBy(string orderColumn, string orderType)
        {
            Type typeQueryable = typeof(IQueryable<T>);
            ParameterExpression argQueryable = Expression.Parameter(typeQueryable, "p");
            var outerExpression = Expression.Lambda(argQueryable, argQueryable);
            string[] props = orderColumn.Split('.');
            IQueryable<T> query = new List<T>().AsQueryable<T>();
            Type type = typeof(T);
            ParameterExpression arg = Expression.Parameter(type, "x");

            Expression expr = arg;
            foreach (string prop in props)
            {
                PropertyInfo pi = type.GetProperty(prop, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }

            LambdaExpression lambda = Expression.Lambda(expr, arg);
            string methodName = orderType == "asc" ? "OrderBy" : "OrderByDescending";

            MethodCallExpression resultExp =
                Expression.Call(typeof(Queryable), methodName, new Type[] { typeof(T), type }, outerExpression.Body, Expression.Quote(lambda));
            var finalLambda = Expression.Lambda(resultExp, argQueryable);
            return (Func<IQueryable<T>, IOrderedQueryable<T>>)finalLambda.Compile();
        }

        /// <summary>
        /// This method is used to add entity in database
        /// </summary>
        /// <remarks>Author: Dave Digvijay</remarks>
        /// <param name="entity">value of entity</param>
        public virtual T Add(T entity)
        {
            this.dbSet.Add(entity);
            return entity;
        }

        /// <summary>
        /// This method is used to remove entity from database
        /// </summary>
        /// <remarks>Author: Dave Digvijay</remarks>
        /// <param name="entity">value of entity</param>
        public virtual void Delete(T entity)
        {
            this.dbSet.Attach(entity);
            this.dbSet.Remove(entity);
        }

        /// <summary>
        /// This method is used to remove entities from database
        /// </summary>
        /// <remarks>Author: Dave Digvijay</remarks>
        /// <param name="where">expression value of where condition</param>
        public virtual void Delete(Expression<Func<T, bool>> where)
        {
            IEnumerable<T> objects = this.dbSet.Where<T>(where).AsEnumerable();
            foreach (T obj in objects)
            {
                this.dbSet.Remove(obj);
            }
        }

        /// <summary>
        /// This method is used to fetch entities from database on basis of id
        /// </summary>
        /// <remarks>Author: Dave Digvijay</remarks>
        /// <param name="id">long value of id</param>
        /// <returns>object type T</returns>
        public virtual T GetById(int id)
        {
            return this.dbSet.Find(id);
        }

        /// <summary>
        /// This method is used to fetch entities from database on basis of id
        /// </summary>
        /// <remarks>Author: Dave Digvijay</remarks>
        /// <param name="id">string value of id</param>
        /// <returns>object type T</returns>
        public virtual T GetById(string id)
        {
            return this.dbSet.Find(id);
        }

        /// <summary>
        /// This method is used to fetch all entities from database
        /// </summary>
        /// <remarks>Author: Dave Digvijay</remarks>
        /// <returns>list of type T</returns>
        public virtual List<T> GetAll()
        {
            return this.dbSet.ToList();
        }

        /// <summary>
        /// This method is used to fetch multiple entities from database on basis of expression provided
        /// </summary>
        /// <remarks>Author: Dave Digvijay</remarks>
        /// <param name="where">expression value of where condition</param>
        /// <returns>list of type T</returns>
        public virtual IEnumerable<T> GetMany(Expression<Func<T, bool>> where)
        {
            return this.dbSet.Where(where).ToList();
        }

        /// <summary>
        /// This method is used to fetch entities from database on basis of expression provided
        /// </summary>
        /// <remarks>Author: Dave Digvijay</remarks>
        /// <param name="where">expression value of where condition</param>
        /// <returns>object of type T</returns>
        public T Get(Expression<Func<T, bool>> where)
        {
            return this.dbSet.Where(where).FirstOrDefault<T>();
        }

        /// <summary>
        /// This method is used to fetch entities from database on basis of expression provided
        /// </summary>
        /// <remarks>Author: Dave Digvijay</remarks>
        /// <param name="where">expression value of where condition</param>
        /// <returns>object of type T</returns>
        public T Get(Expression<Func<T, bool>> where, string includeProperties = "")
        {
            IQueryable<T> query = this.dbSet.Where(where);

            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            return query.FirstOrDefault<T>();
        }

        /// <summary>
        /// This method is used to fetch selective columns from db on the basis of sxpression provided
        /// </summary>
        /// <remarks>Author: Kim Kavita</remarks>
        /// <typeparam name="T1">Selective columns type</typeparam>
        /// <param name="exp">expression value of where condition</param>
        /// <param name="columns">expression value for columns select</param>
        /// <returns>list of new type selective columns</returns>
        public IEnumerable<T1> GetBy<T1>(Expression<Func<T, bool>> exp, Expression<Func<T, T1>> columns)
        {
            return this.dbSet.Where<T>(exp).Select<T, T1>(columns);
        }
    }
}
