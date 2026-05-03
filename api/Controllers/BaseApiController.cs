using api.Helper;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseApiController : ControllerBase
    {
        protected void AddPaginationHeaders(PaginationMetadata metadata)
        {
            Response.Headers.Append("X-Pagination-Total-Count", metadata.TotalCount.ToString());
            Response.Headers.Append("X-Pagination-Page", metadata.CurrentPage.ToString());
            Response.Headers.Append("X-Pagination-Page-Size", metadata.PageSize.ToString());
            Response.Headers.Append("X-Pagination-Total-Pages", metadata.TotalPages.ToString());
            Response.Headers.Append("X-Pagination-Has-Next", (metadata.CurrentPage < metadata.TotalPages).ToString().ToLower());
            Response.Headers.Append("X-Pagination-Has-Previous", (metadata.CurrentPage > 1).ToString().ToLower());
        }
    }
}
