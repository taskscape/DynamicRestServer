using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dynamic.DbScaffolder
{
    public static class ScaffolderHelper
    {
        public static Type GetScaffoldedDbContextType()
            => AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.Contains(RuntimeScaffolder.DbContextName))
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.FullName == $"{typeof(RuntimeScaffolder).Namespace}.{RuntimeScaffolder.DbContextName}");

        public static IEnumerable<Type> GetScaffoldedDbContextEntityTypes()
            => AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.Contains(RuntimeScaffolder.DbContextName))
                .SelectMany(a => a.GetExportedTypes().Where(t => t.BaseType != typeof(DbContext)));

        public static IEnumerable<Type> GetDtosTypes()
            => AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.Contains(RuntimeScaffolder.DtosAssemblyName))
                .SelectMany(a => a.GetExportedTypes());
    }
}
