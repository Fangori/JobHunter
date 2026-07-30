namespace JobHunter.API.Exceptions;

// Loi nghiep vu (QD/TS/BR) -> AuthController/... bat va tra dung status code + { "message": "..." }
public class BusinessRuleException : Exception
{
    public int StatusCode { get; }

    public BusinessRuleException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}
