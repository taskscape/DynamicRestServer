using Swashbuckle.AspNetCore.Annotations;

namespace Dynamic.Shared.Queries
{
    public class GetQuery : IShapeableQuery
    {
        [SwaggerParameter("Comma separated related entities to include. Nested properties can be appended, separated by the '.' character. Case sensitive.")]
        public string Include { get; set; }

        [SwaggerParameter("Comma separated properties to return")]
        public string Select { get; set; }
    }
}
