using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
public class LoggingMiddleware {
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger) {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context) {
        var requestId = Guid.NewGuid().ToString();
        context.Items["RequestId"] = requestId;
        _logger.LogInformation("Request {RequestId} starting: {Path}", requestId, context.Request.Path);

        try {
            await _next(context);
        } catch (Exception ex) {
            _logger.LogError(ex, "Unhandled exception for {RequestId}", requestId);
            throw;
        }

        _logger.LogInformation("Request {RequestId} completed", requestId);
    }
}