namespace FamilyTree.Errors;

public class ErrorResponse
{
    public ErrorResponse(string errorCode, string errorDescription)
    {
        ErrorCode = errorCode;
        ErrorDescription = errorDescription;
    }

    public string ErrorCode { get; set; }
    public string ErrorDescription { get; set; }
}
