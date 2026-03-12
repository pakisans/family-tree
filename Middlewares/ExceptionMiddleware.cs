using System.Net.Mime;
using System.Text.Json;
using FamilyTree.Errors;

namespace FamilyTree.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (HttpResponseException ex)
        {
            context.Response.ContentType = MediaTypeNames.Application.Json;
            context.Response.StatusCode = (int)ex.StatusCode;

            await context.Response.WriteAsync(JsonSerializer.Serialize(ex.ErrorResponse));
        }
        catch (Exception)
        {
            context.Response.ContentType = MediaTypeNames.Application.Json;
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            ErrorResponse response = new ErrorResponse("SERVER_5000", "Internal server error.");
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
