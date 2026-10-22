using BusTicketStore.Application.Exceptions;
using BusTicketStore.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BusTicketStore.Api.Middleware;

/// <summary>
/// The single place where an exception becomes an HTTP status code.
/// </summary>
/// <remarks>
/// Minimal APIs translate nothing by themselves. Without this handler a malformed JSON
/// body or an unhandled validation failure surfaces as 500, which tells the client to
/// retry something that will never succeed.
///
/// Handling the translation here instead of per endpoint is what keeps the error contract
/// uniform. The first endpoint that catches its own exceptions becomes the precedent, and
/// within a few use cases the API emits four different error shapes.
///
/// The namespace is <c>BusTicketStore.Api</c> even though the project folder is still
/// <c>backend/BusTicketStore</c>: SPEC-002 relocates the project to
/// <c>backend/src/BusTicketStore.Api</c> without changing this namespace, so the rename
/// does not touch this file.
/// </remarks>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = Map(exception);

        // The exception is logged once, here, with its stack trace. If every endpoint
        // logged its own failures the traces would be duplicated and the log format would
        // drift from file to file.
        logger.LogError(
            exception,
            "Request {Method} {Path} failed with {StatusCode}.",
            httpContext.Request.Method,
            httpContext.Request.Path,
            statusCode);

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = exception.Message,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
                Extensions = GetExtensions(exception)
            }
        });
    }

    /// <summary>
    /// The status mapping table.
    /// </summary>
    /// <remarks>
    /// Each code is chosen from the semantics of the failure, not from how severe it looks.
    ///
    /// 400 for a validation failure: the payload shape was wrong, and retrying it
    /// unchanged cannot succeed.
    ///
    /// 422 for a domain rule violation: the JSON was well formed and every field passed its
    /// format rules, but the request contradicts a business rule, such as an unpriced fare
    /// combination or a stop that does not belong to the route. 422 says "understood, but
    /// refused", where 400 would blame the syntax and 500 would blame the server.
    ///
    /// 400 for <c>BadHttpRequestException</c>: it is thrown while reading the request body,
    /// so a truncated or malformed JSON document lands here.
    ///
    /// 499 for a cancelled request: the client hung up. Reporting that as 500 would blame
    /// the server for a client-side disconnection.
    ///
    /// Anything unmapped is a genuine server fault and stays 500.
    /// </remarks>
    private static (int StatusCode, string Title) Map(Exception exception) => exception switch
    {
        ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
        BadHttpRequestException => (StatusCodes.Status400BadRequest, "Malformed request"),
        DomainException => (StatusCodes.Status422UnprocessableEntity, "Domain rule violated"),
        OperationCanceledException => (499, "Request cancelled by the client"),
        _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
    };

    /// <summary>
    /// Field-level failures travel to the client; the internals of an unexpected error do
    /// not.
    /// </summary>
    /// <remarks>
    /// A stack trace or an internal message inside a 500 response is an information leak,
    /// so the extensions stay empty for every unmapped exception.
    /// </remarks>
    private static Dictionary<string, object?> GetExtensions(Exception exception) => exception switch
    {
        ValidationException validation => new Dictionary<string, object?>
        {
            ["errors"] = validation.Errors.ToDictionary(pair => pair.Key, pair => pair.Value)
        },
        _ => new Dictionary<string, object?>()
    };
}
