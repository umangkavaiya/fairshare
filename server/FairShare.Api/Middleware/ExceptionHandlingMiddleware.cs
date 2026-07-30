using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using FairShare.Application.DTOs.Common;

namespace FairShare.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var traceId = context.TraceIdentifier;

        var (statusCode, message, code) = ex switch
        {
            DbUpdateConcurrencyException => (HttpStatusCode.Conflict, "Record has been modified by another user.", "CONCURRENCY_CONFLICT"),
            UnauthorizedAccessException => (HttpStatusCode.Forbidden, ex.Message, "FORBIDDEN"),
            KeyNotFoundException => (HttpStatusCode.NotFound, ex.Message, "NOT_FOUND"),
            ArgumentException => (HttpStatusCode.BadRequest, ex.Message, "BAD_REQUEST"),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", "INTERNAL_ERROR")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", traceId);
        else
            _logger.LogWarning("Handled exception: {Message}. TraceId: {TraceId}", ex.Message, traceId);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new ApiErrorResponse
        {
            Message = message,
            Code = code,
            TraceId = traceId
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}