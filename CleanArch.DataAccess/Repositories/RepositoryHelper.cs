using System.Linq.Expressions;

namespace CleanArch.Infrastructure.Repositories
{
    public static class RepositoryHelper
    {
        public static Expression<Func<T, bool>> GenerateNameFilterExpression<T>(string search)
        {
            // Get the "Name" property of the entity
            var parameter = Expression.Parameter(typeof(T), "x");
            var nameProperty = Expression.Property(parameter, "Name"); // Assumes entities have a "Name" property

            // Create expression: x.Name.Contains(search)
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var searchExpression = Expression.Constant(search, typeof(string));
            var containsExpression = Expression.Call(nameProperty, containsMethod, searchExpression);

            return Expression.Lambda<Func<T, bool>>(containsExpression, parameter);
        }


        public static Expression<Func<T, bool>> GenerateInStockFilterExpression<T>(bool inStock)
        {
            // Get the "InStock" property of the entity
            var parameter = Expression.Parameter(typeof(T), "x");
            var inStockProperty = Expression.Property(parameter, "InStock"); // Assumes entities have an "InStock" property

            // Create expression: x.InStock == inStock
            var inStockExpression = Expression.Equal(inStockProperty, Expression.Constant(inStock));

            return Expression.Lambda<Func<T, bool>>(inStockExpression, parameter);
        }

        public static Expression<Func<T, bool>> GenerateIsNewFilterExpression<T>(bool isNew)
        {
            // Get the "IsNew" property of the entity
            var parameter = Expression.Parameter(typeof(T), "x");
            var isNewProperty = Expression.Property(parameter, "IsNew"); // Assumes entities have an "IsNew" property
            // Create expression: x.IsNew == isNew
            var isNewExpression = Expression.Equal(isNewProperty, Expression.Constant(isNew));
            return Expression.Lambda<Func<T, bool>>(isNewExpression, parameter);
        }
    }
}
