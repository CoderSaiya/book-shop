namespace BookShop.Domain.Common;

public class GlobalResponse<T>(T? data, string message, int statusCode)
{
    public T? Data { get; set; } = data;
    public string Message { get; set; } = message;
    public int StatusCode { get; set; } = statusCode;
    public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;

    public GlobalResponse(string message, int statusCode) : this(default(T?), message, statusCode)
    {
    }

    public static GlobalResponse<T?> Success(T? data, string message = "Request successful", int statusCode = 200)
        => new(data, message, statusCode);

    public static GlobalResponse<T> Error(string message, int statusCode = 400)
        => new(message, statusCode);
}