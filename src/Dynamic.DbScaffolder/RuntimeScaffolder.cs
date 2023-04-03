using Dynamic.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.Diagnostics.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Scaffolding.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace Dynamic.DbScaffolder
{
    public enum ScaffolderMode
    {
        Mssql,
        Postgre
    }

    public class RuntimeScaffolder
    {
        public static string DbContextName { get; } = "ScaffoldedDbContext";
        public static string DtosAssemblyName { get; } = "Dtos";

        private readonly ILogger<RuntimeScaffolder> _logger;
        private readonly string _connectionString;
        private readonly bool _enableLazyLoading;
        private readonly ScaffolderMode _scaffolderMode;

        public RuntimeScaffolder(ILogger<RuntimeScaffolder> logger, string connectionString, bool enableLazyLoading)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            _connectionString = connectionString;
            _enableLazyLoading = enableLazyLoading;
            _scaffolderMode = _connectionString.Contains("postgres") ? ScaffolderMode.Postgre : ScaffolderMode.Mssql;
        }

        public void Start()
        {
            var scaffolder = _scaffolderMode == ScaffolderMode.Postgre ? CreatePostgreScaffolder() : CreateMssqlScaffolder();

            var assemblyLoadContext = new AssemblyLoadContext(null, isCollectible: !_enableLazyLoading);

            _logger.LogInformation("{ScaffolderMode}Scaffolder created", _scaffolderMode);

            GenerateDynamicDbContext(scaffolder, assemblyLoadContext);
            GenerateDtos(assemblyLoadContext);

            _logger.LogInformation("Dynamically created assemblies loaded");

            if (!_enableLazyLoading)
            {
                assemblyLoadContext.Unload();
            }
        }

        private void GenerateDynamicDbContext(IReverseEngineerScaffolder scaffolder, AssemblyLoadContext assemblyLoadContext)
        {
            var dbOpts = new DatabaseModelFactoryOptions();
            var modelOpts = new ModelReverseEngineerOptions();
            var codeGenOpts = new ModelCodeGenerationOptions()
            {
                RootNamespace = GetType().Namespace,
                ContextName = DbContextName,
                ContextNamespace = GetType().Namespace,
                ModelNamespace = $"{GetType().Namespace}.Entities",
                SuppressConnectionStringWarning = true,
                UseDataAnnotations = true
            };

            var scaffoldedModelSources = scaffolder.ScaffoldModel(_connectionString, dbOpts, modelOpts, codeGenOpts);
            _logger.LogInformation("Db model scaffolded");

            var contextFile = scaffoldedModelSources.ContextFile.Code;
            contextFile = AddConstructor(contextFile);
            contextFile = UpdateOnConfiguring(contextFile);

            if (_scaffolderMode == ScaffolderMode.Postgre)
            {
                contextFile = UpdatePostgreUsings(contextFile);
            }

            if (_enableLazyLoading)
            {
                contextFile = scaffoldedModelSources.ContextFile.Code.Replace(".UseSqlServer", ".UseLazyLoadingProxies().UseSqlServer");
            }

            var sourceFiles = new List<string> { contextFile };
            sourceFiles.AddRange(scaffoldedModelSources.AdditionalFiles.Select(f => f.Code));

            using (var peStream = new MemoryStream())
            {
                var references = DbContextReferences();
                var result = GenerateCode(DbContextName, sourceFiles, references).Emit(peStream);

                CheckEmitedResult(result);

                peStream.Seek(0, SeekOrigin.Begin);
                assemblyLoadContext.LoadFromStream(peStream);
            };

            _logger.LogInformation("DynamicDbContext generated");
        }

        private string AddConstructor(string contextFile)
        {
            contextFile = contextFile.Replace(@$"
        public {DbContextName}()
        {{
        }}
", @$"
        private readonly Microsoft.Extensions.Logging.ILoggerFactory _loggerFactory;

        public {DbContextName}()
        {{
        }}

        public {DbContextName}(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
        {{
            _loggerFactory = loggerFactory;
        }}

        public {DbContextName}(DbContextOptions options) : base(options)
        {{
        }}
");
            return contextFile;
        }

        private string UpdateOnConfiguring(string contextFile)
        {
            contextFile = contextFile.Replace(@$"
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {{
", @$"
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {{
            optionsBuilder.UseLoggerFactory(_loggerFactory);
            optionsBuilder.ConfigureWarnings(x => x.Log(Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.InvalidIncludePathError));
");
            return contextFile;
        }

        private static string UpdatePostgreUsings(string contextFile)
        {
            contextFile = contextFile.Replace(@$"
using Microsoft.EntityFrameworkCore.Metadata;
", @$"
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
");
            return contextFile;
        }

        private List<MetadataReference> DbContextReferences()
        {
            var refs = new List<MetadataReference>();
            var referencedAssemblies = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
            refs.AddRange(referencedAssemblies.Select(a => MetadataReference.CreateFromFile(Assembly.Load(a).Location)));

            refs.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            refs.Add(MetadataReference.CreateFromFile(Assembly.Load("netstandard, Version=2.0.0.0").Location));
            refs.Add(MetadataReference.CreateFromFile(typeof(System.Data.Common.DbConnection).Assembly.Location));
            refs.Add(MetadataReference.CreateFromFile(typeof(System.Linq.Expressions.Expression).Assembly.Location));
            refs.Add(MetadataReference.CreateFromFile(typeof(KeyAttribute).Assembly.Location));
            refs.Add(MetadataReference.CreateFromFile(typeof(IndexAttribute).Assembly.Location));

            if (_enableLazyLoading)
            {
                refs.Add(MetadataReference.CreateFromFile(typeof(ProxiesExtensions).Assembly.Location));
            }

            return refs;
        }

        private CSharpCompilation GenerateCode(string assemblyName, IEnumerable<string> sourceFiles, IEnumerable<MetadataReference> references)
        {
            var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp9);
            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release,
                assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default);
            var parsedSyntaxTrees = sourceFiles.Select(f => SyntaxFactory.ParseSyntaxTree(f, parseOptions));

            return CSharpCompilation.Create($"{assemblyName}.dll", parsedSyntaxTrees, references, options: compilationOptions);
        }

        private void CheckEmitedResult(EmitResult result)
        {
            if (!result.Success)
            {
                var failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);
                var error = failures.FirstOrDefault();

                throw new Exception($"{error?.Id}: {error?.GetMessage()}");
            }
        }

        private void GenerateDtos(AssemblyLoadContext assemblyLoadContext)
        {
            var assembly = assemblyLoadContext.Assemblies.First();
            var sourceFiles = new List<string>();
            var builder = new StringBuilder();

            foreach (var exportedType in assembly.ExportedTypes.Where(t => t.BaseType != typeof(DbContext)))
            {
                var properties = exportedType.GetProperties();

                var dto = BuildDto(builder, exportedType, properties);
                sourceFiles.Add(dto);
                builder.Clear();

                var flatDto = BuildDto(builder, exportedType, properties, false, true);
                sourceFiles.Add(flatDto);
                builder.Clear();

                var editDto = BuildDto(builder, exportedType, properties, true);
                sourceFiles.Add(editDto);
                builder.Clear();
            }

            using (var peStream = new MemoryStream())
            {
                var references = DtosReferences();
                var result = GenerateCode(DtosAssemblyName, sourceFiles, references).Emit(peStream);

                CheckEmitedResult(result);

                peStream.Seek(0, SeekOrigin.Begin);
                assemblyLoadContext.LoadFromStream(peStream);
            };

            _logger.LogInformation("Dtos generated");
        }

        private string BuildDto(StringBuilder builder, Type exportedType, IEnumerable<PropertyInfo> properties, bool editable = false, bool isFlat = false)
        {
            if (editable)
            {
                builder.Append("using System.ComponentModel.DataAnnotations;");
                builder.AppendLine();
            }

            builder.Append("using System.Collections.Generic;");
            builder.AppendLine();
            builder.Append($"namespace {GetType().Namespace}.Dto");
            builder.AppendLine();
            builder.Append('{');
            builder.AppendLine();

            if (editable)
            {
                builder.Append($"public class {exportedType.Name}EditDto");
            }
            else if (isFlat)
            {
                builder.Append($"public class {exportedType.Name}FlatDto");
            }
            else
            {
                builder.Append($"public class {exportedType.Name}Dto");
            }

            builder.AppendLine();
            builder.Append('{');
            builder.AppendLine();

            foreach (var property in properties)
            {
                if (property.IsVirtual())
                {
                    if (property.IsGenericCollection())
                    {
                        var collectionType = isFlat ? "ICollection<object>" : $"ICollection<{property.GetGenericArgument().Name}FlatDto>";
                        builder.Append($"public {collectionType} {property.Name} {{ get; set; }}");
                    }
                    else
                    {
                        var propertyType = isFlat ? "object" : $"{property.PropertyType.Name}FlatDto";
                        builder.Append($"public {propertyType} {property.Name} {{ get; set; }}");
                    }
                }
                else
                {
                    if (editable)
                    {
                        AddValidationAttribute(builder, property);
                    }

                    if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        var underlyingType = Nullable.GetUnderlyingType(property.PropertyType);
                        builder.Append($"public {underlyingType.FullName}? {property.Name} {{ get; set; }}");
                    }
                    else
                    {
                        if (editable && property.HasKeyAttribute())
                        {
                            continue;
                        }
                        else
                        {
                            builder.Append($"public {property.PropertyType.FullName} {property.Name} {{ get; set; }}");
                        }
                    }
                }

                builder.AppendLine();
            }

            builder.Append('}');
            builder.AppendLine();
            builder.Append('}');

            return builder.ToString();
        }

        private static void AddValidationAttribute(StringBuilder builder, PropertyInfo property)
        {
            var validationAttributes = property.CustomAttributes.Where(x => x.AttributeType.BaseType == typeof(ValidationAttribute));
            if (validationAttributes.Any())
            {
                var lengthAttribute = property.GetCustomAttribute<StringLengthAttribute>(false);
                var requiredAttribute = property.GetCustomAttribute<RequiredAttribute>(false);

                if (lengthAttribute?.MaximumLength > 0)
                {
                    builder.Append($"[StringLength({lengthAttribute.MaximumLength})]");
                    builder.AppendLine();
                }

                if (requiredAttribute is not null)
                {
                    builder.Append("[Required]");
                    builder.AppendLine();
                }
            }
        }

        private List<MetadataReference> DtosReferences()
        {
            var refs = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            };

            var referencedAssemblies = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
            refs.AddRange(referencedAssemblies.Select(a => MetadataReference.CreateFromFile(Assembly.Load(a).Location)));
            refs.Add(MetadataReference.CreateFromFile(Assembly.Load("netstandard, Version=2.0.0.0").Location));

            return refs;
        }

        [SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "I need it")]
        private IReverseEngineerScaffolder CreateMssqlScaffolder() =>
            new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddLogging()
                .AddEntityFrameworkDesignTimeServices()
                .AddSingleton<LoggingDefinitions, SqlServerLoggingDefinitions>()
                .AddSingleton<IRelationalTypeMappingSource, SqlServerTypeMappingSource>()
                .AddSingleton<IAnnotationCodeGenerator, AnnotationCodeGenerator>()
                .AddSingleton<IDatabaseModelFactory, SqlServerDatabaseModelFactory>()
                .AddSingleton<IProviderConfigurationCodeGenerator, SqlServerCodeGenerator>()
                .AddSingleton<IScaffoldingModelFactory, RelationalScaffoldingModelFactory>()
                .BuildServiceProvider()
                .GetRequiredService<IReverseEngineerScaffolder>();


        [SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "Needed")]
        private IReverseEngineerScaffolder CreatePostgreScaffolder() =>
            new ServiceCollection()
                .AddEntityFrameworkNpgsql()
                .AddLogging()
                .AddEntityFrameworkDesignTimeServices()
                .AddSingleton<LoggingDefinitions, NpgsqlLoggingDefinitions>()
                .AddSingleton<IRelationalTypeMappingSource, NpgsqlTypeMappingSource>()
                .AddSingleton<IAnnotationCodeGenerator, AnnotationCodeGenerator>()
                .AddSingleton<IDatabaseModelFactory, NpgsqlDatabaseModelFactory>()
                .AddSingleton<IProviderConfigurationCodeGenerator, NpgsqlCodeGenerator>()
                .AddSingleton<IScaffoldingModelFactory, RelationalScaffoldingModelFactory>()
                .BuildServiceProvider()
                .GetRequiredService<IReverseEngineerScaffolder>();
    }
}
