using Dynamic.Shared.Exceptions;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Dynamic.Api.Middleware
{
    internal class ErrorHandlerMiddleware : IMiddleware
    {
        private readonly ConcurrentDictionary<Type, string> _codes = new();
        private readonly ILogger<ErrorHandlerMiddleware> _logger;

        public ErrorHandlerMiddleware(ILogger<ErrorHandlerMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                var statusCode = 500;
                var code = "error";
                var message = "There was an unexpected error.";

                _logger.LogError(exception, exception.Message);

                if (exception is CustomException customException)
                {
                    statusCode = customException switch
                    {
                        EntityNotFoundException _ => 404,
                        NonExistentPropertyException _ => 404,
                        _ => 400
                    };
                    var exceptionType = customException.GetType();

                    if (!_codes.TryGetValue(exceptionType, out var errorCode))
                    {
                        code = customException.GetType().Name.Underscore().Replace("_exception", string.Empty);
                        _codes.TryAdd(exceptionType, code);
                    }
                    else
                    {
                        code = errorCode;
                    }

                    message = customException.Message;
                }

                context.Response.StatusCode = statusCode;
                await context.Response.WriteAsJsonAsync(new ApiError { Code = code, Message = message });
            }
        }
    }
}
