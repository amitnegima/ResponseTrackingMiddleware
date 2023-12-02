namespace LoggingMiddleWare.Middleware
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using System.IO;
    using System.Threading.Tasks;

    public class ResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ResponseLoggingMiddleware> _logger;

        public ResponseLoggingMiddleware(RequestDelegate next, ILogger<ResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            // Capture the response body
            var originalBodyStream = context.Response.Body;
            string requestBody=string.Empty;    
            using (StreamReader reader = new StreamReader(context.Request.Body))
            {
                 requestBody = await reader.ReadToEndAsync();
            }

            using (var memoryStream = new MemoryStream())
            {
                context.Response.Body = memoryStream;
                var requestStream = context.Request.BodyReader;

                await _next(context);

                memoryStream.Seek(0, SeekOrigin.Begin);
                var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();
                var method = context.Request.Method;
                var path = context.Request.Path;
                var query = context.Request.QueryString;

                // Log the response
                _logger.LogInformation($"Endpoint: {context.Request.Path}, method:{method} ,path:{path} ,query:{query} Request Body:{requestBody} Response: {responseBody}");

                memoryStream.Seek(0, SeekOrigin.Begin);
                await memoryStream.CopyToAsync(originalBodyStream);
            }
        }
    }

    public static class ResponseLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseResponseLoggingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ResponseLoggingMiddleware>();
        }
    }

}
