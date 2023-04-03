using Dynamic.Api.Attributes;
using Dynamic.Services;
using Dynamic.Shared.Pagination;
using Dynamic.Shared.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Dynamic.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [BaseImplemetation(true)]
    public class GenericController<TEntity, TDto, TEditDto> : ControllerBase where TEntity : class where TDto : class where TEditDto : class
    {
        protected readonly IGenericService<TEntity, TDto, TEditDto> Service;

        public GenericController(IGenericService<TEntity, TDto, TEditDto> service)
        {
            Service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Retrieves a paged collection of items based on query
        /// </summary>
        /// <param name="query">Query to pagination</param>
        /// <returns>A paged collection of items</returns>
        [HttpGet]
        [Route("paged")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation(Description = "Reads a paged collection of items")]
        public virtual async Task<ActionResult<PaginatedResult<TDto>>> GetPagedResultAsync([FromQuery] PaginatedQuery query)
        {
            return Ok(await Service.GetPagedResultAsync(query));
        }

        /// <summary>
        /// Retrieves a collection of items
        /// </summary>
        /// <param name="listQuery">Query</param>
        /// <returns>A collection of items</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation(Description = "Reads all items")]
        public virtual async Task<ActionResult<IReadOnlyList<TDto>>> BrowseAsync([FromQuery] ListQuery listQuery)
        {
            if (!string.IsNullOrWhiteSpace(listQuery.Select))
            {
                return Ok(await Service.BrowseDynamicAsync(listQuery));
            }

            return Ok(await Service.BrowseAsync(listQuery));
        }

        /// <summary>
        /// Retrieves a specific item by unique id
        /// </summary>
        /// <param name="id">The unique identifier</param>
        /// <param name="getQuery">Query</param>
        /// <returns>An item with specified identifier</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [SwaggerOperation(Description = "Reads a specific item")]
        public virtual async Task<ActionResult<TDto>> GetAsync(int id, [FromQuery] GetQuery getQuery)
        {
            if (!string.IsNullOrWhiteSpace(getQuery.Select))
            {
                var dynamicObject = await Service.GetDynamicAsync(id, getQuery);

                if (dynamicObject is null)
                {
                    return NotFound();
                }

                return Ok(dynamicObject);
            }

            var dto = await Service.GetAsync(id, getQuery);

            if (dto is null)
            {
                return NotFound();
            }

            return Ok(dto);
        }

        /// <summary>
        /// Creates a new item from posted dto
        /// </summary>
        /// <param name="editDto">The item to be added</param>
        /// <returns>A 201 http response with link to get newly created item</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [SwaggerOperation(Description = "Creates a new item")]
        public virtual async Task<ActionResult> AddAsync(TEditDto editDto)
        {
            var id = await Service.AddAsync(editDto);

            return CreatedAtAction(nameof(GetAsync), new { id }, null);
        }

        /// <summary>
        /// Updates a specific item from posted dto
        /// </summary>
        /// <param name="id">The unique identifier</param>
        /// <param name="editDto">The item to be updated from</param>
        /// <returns>No content</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [SwaggerOperation(Description = "Updates a specific item")]
        public virtual async Task<ActionResult> UpdateAsync(int id, TEditDto editDto)
        {
            await Service.UpdateAsync(id, editDto);
            return NoContent();
        }

        /// <summary>
        /// Deletes a specific item
        /// </summary>
        /// <param name="id">The unique identifier</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [SwaggerOperation(Description = "Deletes a specific item")]
        public virtual async Task<ActionResult> DeleteAsync(int id)
        {
            await Service.DeleteAsync(id);

            return NoContent();
        }
    }
}
