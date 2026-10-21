namespace MiniApi.Exceptions;

public class ForbiddenException : Exception
{
    public string ErrorCode { get; }
    public ForbiddenException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}