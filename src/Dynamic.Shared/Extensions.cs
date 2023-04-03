using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace Dynamic.Shared
{
    public static class Extensions
    {
        public static bool IsOverride(this MethodInfo methodInfo) => methodInfo.GetBaseDefinition().DeclaringType != methodInfo.DeclaringType;
        public static IEnumerable<PropertyInfo> GetNonVirtualProperties(this Type type) => type.GetProperties().Where(p => p.GetAccessors().All(m => !m.IsVirtual));

        public static bool IsVirtual(this PropertyInfo propertyInfo) => propertyInfo.GetGetMethod().IsVirtual;

        public static bool IsGenericCollection(this PropertyInfo propertyInfo)
            => propertyInfo.PropertyType.IsGenericType && typeof(ICollection<>).IsAssignableFrom(propertyInfo.PropertyType.GetGenericTypeDefinition());

        public static bool HasKeyAttribute(this PropertyInfo propertyInfo) => propertyInfo.GetCustomAttribute<KeyAttribute>() is not null;

        public static string GetKeyName(this Type type) => type.GetProperties().FirstOrDefault(x => x.GetCustomAttribute<KeyAttribute>() is not null).Name;

        public static bool HasInversePropertyAttribute(this PropertyInfo propertyInfo) => propertyInfo.GetCustomAttribute<InversePropertyAttribute>() is not null;

        public static Type GetGenericArgument(this PropertyInfo propertyInfo) => propertyInfo.PropertyType.GetGenericArguments()[0];

        public static int GetIdValue(this object obj)
        {
            var type = obj.GetType();
            var keyName = type.GetKeyName();
            var result = type.GetProperty(keyName).GetValue(obj);

            return Convert.ToInt32(result);
        }
    }
}
