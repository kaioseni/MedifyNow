using Microsoft.AspNetCore.Http;

public class KeyMiddleware
{
    private const string KeyName = "x-api-key";
    private readonly RequestDelegate _next;

    public KeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {

        if (!context.Request.Headers.TryGetValue(KeyName, out Microsoft.Extensions.Primitives.StringValues value))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Chave não informada");
            return;
        }

        string DesiredKey = context.RequestServices.GetRequiredService<IConfiguration>().GetValue<string>(KeyName);

        if (DesiredKey != value)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Chave errada");
            return;
        }

        await _next(context);
    }
}

public static class KeyMiddlewareExtensions
{
    public static IApplicationBuilder UseAPIKey(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<KeyMiddleware>();
    }
}