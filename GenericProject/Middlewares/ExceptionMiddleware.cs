using System.Net;
using System.Text.Json;
using System;
using GenericProject.Contracts.ResponseModels;
using GenericProject.Domain.Exceptions;
using FluentValidation;// ValidationException

namespace GenericProject.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        //private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, IHostEnvironment env)
        {
            _next = next;
           // _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            ApiResponse<object> response; // Data null olacak, sadece hata mesajları önemli

            switch (exception)
            {
                case ValidationException validationException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest; // 400
                    var errors = validationException.Errors.Select(e => e.ErrorMessage).ToList();
                    response = ApiResponse<object>.Fail(errors, context.Response.StatusCode);
                    break;

                case NotFoundException notFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound; // 404
                    response = ApiResponse<object>.Fail(notFoundException.Message, context.Response.StatusCode);
                    break;

                // Buraya başka özel Exception türleri eklenebilir (örn: UnauthorizedAccessException -> 401)
                case UnauthorizedAccessException unauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized; // 401
                    response = ApiResponse<object>.Fail(unauthorizedAccessException.Message, context.Response.StatusCode);
                    break;

                case ArgumentException argumentException: // Örneğin yanlış parametre
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest; // 400
                    response = ApiResponse<object>.Fail(argumentException.Message, context.Response.StatusCode);
                    break;

                default: // Yakalanamayan diğer tüm hatalar için
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; // 500
                    var errorMessage = _env.IsDevelopment()
                    ? $"Internal Server Error: {exception.Message} {exception.StackTrace}" // Geliştirme ortamında detaylı hata
                        : "An unexpected internal server error occurred."; // Prod ortamında genel hata
                    response = ApiResponse<object>.Fail(errorMessage, context.Response.StatusCode);
                    break;
            }
       


        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase // camelCase JSON çıktısı için
        });

        await context.Response.WriteAsync(jsonResponse);

        }
    }
}
