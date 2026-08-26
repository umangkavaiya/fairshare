using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using FairShare.Api.Extensions;
using FairShare.Domain.Entities;
using FairShare.Infrastructure.Data;

namespace FairShare.Api.Filters;

// Apply to POST endpoints where a duplicate submission has real consequences
// (expense creation, settlement creation). Not needed everywhere — see Phase 2.5 notes.
public class IdempotencyFilterAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var httpContext = context.HttpContext;

        if (!httpContext.Request.Headers.TryGetValue("Idempotency-Key", out var keyHeader) || string.IsNullOrWhiteSpace(keyHeader))
        {
            await next();
            return;
        }

        var key = keyHeader.ToString();
        var userId = httpContext.User.GetUserId();
        var db = httpContext.RequestServices.GetRequiredService<ApplicationDbContext>();

        var existing = await db.IdempotencyKeys.FirstOrDefaultAsync(k => k.Key == key && k.UserId == userId);
        if (existing is not null && existing.ExpiresAt > DateTime.UtcNow)
        {
            var cached = JsonSerializer.Deserialize<object>(existing.ResponseBody);
            context.Result = new ObjectResult(cached) { StatusCode = existing.ResponseStatusCode };
            return;
        }

        var executed = await next();

        if (executed.Result is ObjectResult { StatusCode: >= 200 and < 300 } objectResult)
        {
            db.IdempotencyKeys.Add(new IdempotencyKey
            {
                Key = key,
                UserId = userId,
                ResponseStatusCode = objectResult.StatusCode ?? 200,
                ResponseBody = JsonSerializer.Serialize(objectResult.Value),
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            });
            await db.SaveChangesAsync();
        }
    }
}