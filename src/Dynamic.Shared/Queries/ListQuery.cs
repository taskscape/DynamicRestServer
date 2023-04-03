using Swashbuckle.AspNetCore.Annotations;

namespace Dynamic.Shared.Queries
{
    public class ListQuery : IShapeableQuery, IOrderableQuery
    {
        public string OrderBy { get; set; }

        [SwaggerParameter("asc, desc")]
        public string SortOrder { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }

        [SwaggerParameter("Comma separated related entities to include. Nested properties can be appended, separated by the '.' character. Case sensitive.")]
        public string Include { get; set; }

        [SwaggerParameter("Comma separated properties to return")]
        public string Select { get; set; }

        [SwaggerParameter("Expression to filter records. Allowed operators: =, >, <, >=, <=, &, |")]
        public string Where { get; set; }
    }
}
