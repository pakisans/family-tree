using System.Net;

namespace FamilyTree.Errors;

public class HttpResponseException : Exception
{
    public HttpResponseException(string errorCode, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        : base(BaseErrorCode.GetDescription(errorCode))
    {
        StatusCode = statusCode;
        ErrorResponse = new ErrorResponse(errorCode, BaseErrorCode.GetDescription(errorCode));
    }

    public HttpResponseException(ErrorResponse errorResponse, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        StatusCode = statusCode;
        ErrorResponse = errorResponse;
    }

    public HttpStatusCode StatusCode { get; }
    public ErrorResponse ErrorResponse { get; }
}
