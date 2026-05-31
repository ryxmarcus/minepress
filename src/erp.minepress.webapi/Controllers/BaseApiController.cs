using erp.minepress.frameworks.Common;
using Microsoft.AspNetCore.Mvc;

namespace erp.minepress.webapi.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult OkResponse<T>(T data, string? message = null)
    {
        return Ok(ApiResponse<T>.Ok(data, message));
    }

    protected IActionResult CreatedResponse<T>(T data, string? message = null)
    {
        return StatusCode(201, ApiResponse<T>.Created(data, message));
    }

    protected IActionResult ErrorResponse<T>(string message, int statusCode = 400)
    {
        return StatusCode(statusCode, ApiResponse<T>.Error(message, statusCode));
    }

    protected IActionResult NotFoundResponse<T>(string? message = null)
    {
        return NotFound(ApiResponse<T>.NotFound(message));
    }
}
