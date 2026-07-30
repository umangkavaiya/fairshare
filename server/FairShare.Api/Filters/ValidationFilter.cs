using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using FairShare.Application.DTOs.Common;

namespace FairShare.Api.Filters;

// Every incoming DTO with a registered IValidator<T> is validated automatically here —
// controllers and services never need to call a validator themselves.
public class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationFilter(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var arg in context.ActionArguments.Values)
        {
            if (arg is null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(arg.GetType());
            if (_serviceProvider.GetService(validatorType) is IValidator validator)
            {
                var result = await validator.ValidateAsync(new ValidationContext<object>(arg));
                if (!result.IsValid)
                {
                    context.Result = new BadRequestObjectResult(new ApiErrorResponse
                    {
                        Message = "One or more validation errors occurred.",
                        Code = "VALIDATION_ERROR",
                        TraceId = context.HttpContext.TraceIdentifier,
                        Errors = result.Errors.Select(e => e.ErrorMessage).ToList()
                    });
                    return;
                }
            }
        }

        await next();
    }
}