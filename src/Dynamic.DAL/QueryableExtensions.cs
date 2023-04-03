using Dynamic.Shared;
using Dynamic.Shared.Exceptions;
using Dynamic.Shared.Queries;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Exceptions;

namespace Dynamic.DAL
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> ApplyOrderBy<T>(this IQueryable<T> query, IOrderableQuery orderableQuery) where T : class
        {
            var sortOrder = new string[] { "asc", "desc" };

            if (string.IsNullOrWhiteSpace(orderableQuery.OrderBy))
            {
                orderableQuery.OrderBy = typeof(T).GetKeyName();
            }

            if (string.IsNullOrWhiteSpace(orderableQuery.SortOrder) || !sortOrder.Contains(orderableQuery.SortOrder))
            {
                orderableQuery.SortOrder = "asc";
            }

            try
            {
                return query.OrderBy($"{orderableQuery.OrderBy} {orderableQuery.SortOrder}");
            }
            catch (ParseException ex)
            {
                throw new NonExistentPropertyException(ex.Message);
            }
        }

        public static IQueryable<T> ApplySkip<T>(this IQueryable<T> query, ListQuery listQuery) where T : class
        {
            if (listQuery.Skip > 0)
            {
                query = query.Skip(listQuery.Skip);
            }

            return query;
        }

        public static IQueryable<T> ApplyWhere<T>(this IQueryable<T> query, ListQuery listQuery) where T : class
        {
            if (!string.IsNullOrWhiteSpace(listQuery.Where))
            {
                try
                {
                    query = query.Where(listQuery.Where);
                }
                catch (ParseException ex)
                {
                    throw new NonExistentPropertyException(ex.Message);
                }
            }

            return query;
        }

        public static IQueryable<T> ApplyTake<T>(this IQueryable<T> query, ListQuery listQuery) where T : class
        {
            if (listQuery.Take > 0)
            {
                query = query.Take(listQuery.Take);
            }

            return query;
        }

        public static IQueryable<T> ApplyInclude<T>(this IQueryable<T> query, IShapeableQuery shapeableQuery) where T : class
        {
            if (!string.IsNullOrWhiteSpace(shapeableQuery.Include))
            {
                var includeProperties = shapeableQuery.Include.Split(',');

                foreach (var includeProperty in includeProperties.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    query = query.Include(includeProperty.Sanitize());
                }
            }

            return query;
        }

        public static IQueryable ApplySelect<T>(this IQueryable<T> query, IShapeableQuery shapeableQuery) where T : class
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(shapeableQuery.Select))
                {
                    return query.Select($"new {{ {shapeableQuery.Select} }}");
                }

                return query;
            }
            catch (ParseException ex)
            {
                throw new NonExistentPropertyException(ex.Message);
            }
        }

        private static string Sanitize(this string input)
        {
            var values = input.Split('.').Select(x => x.Replace(" ", string.Empty).Dehumanize());
            return string.Join('.', values);
        }
    }
}
