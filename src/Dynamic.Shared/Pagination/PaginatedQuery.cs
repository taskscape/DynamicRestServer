using Dynamic.Shared.Queries;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.ComponentModel.DataAnnotations;

namespace Dynamic.Shared.Pagination
{
    public class PaginatedQuery : IOrderableQuery
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "The field Page must be greater than {1}")]
        public int Page { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "The field PageSize must be greater than {1}")]
        public int PageSize { get; set; }

        public string OrderBy { get; set; }

        [SwaggerParameter("asc, desc")]
        public string SortOrder { get; set; }
    }
}
