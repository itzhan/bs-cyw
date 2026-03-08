namespace SmartStreetlight.Api.Common;

public class Result<T>
{
    public int Code { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }

    public static Result<T> Success(T? data = default, string? message = null)
        => new() { Code = 200, Message = message ?? "success", Data = data };

    public static Result<T> Error(int code, string message)
        => new() { Code = code, Message = message };
}

public class Result : Result<object>
{
    public static new Result Success(string? message = null)
        => new() { Code = 200, Message = message ?? "success" };

    public static Result Success(string message, object? data)
        => new() { Code = 200, Message = message, Data = data };

    public static new Result Error(int code, string message)
        => new() { Code = code, Message = message };
}
