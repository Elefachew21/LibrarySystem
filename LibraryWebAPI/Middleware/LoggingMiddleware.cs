using System.Diagnostics;
using System.Text;

namespace LibraryWebAPI.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            // Log request
            var request = await FormatRequest(context.Request);
            _logger.LogInformation($"Request: {request}");

            // Copy original response body stream
            var originalBodyStream = context.Response.Body;

            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;

                await _next(context);

                // Log response
                var response = await FormatResponse(context.Response);
                _logger.LogInformation($"Response: {response}");

                await responseBody.CopyToAsync(originalBodyStream);
            }

            stopwatch.Stop();
            _logger.LogInformation($"Request completed in {stopwatch.ElapsedMilliseconds}ms");
        }

        private async Task<string> FormatRequest(HttpRequest request)
        {
            request.EnableBuffering();

            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var bodyAsText = Encoding.UTF8.GetString(buffer);
            request.Body.Position = 0;

            return $"{request.Method} {request.Path}{request.QueryString} {bodyAsText}";
        }

        private async Task<string> FormatResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            return $"Status: {response.StatusCode} - {text}";
        }
    }
}