namespace ShoppingApi.Services;

public class ShopException : Exception
{
    public ShopException(string message, int statusCode = StatusCodes.Status400BadRequest)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; }
}
