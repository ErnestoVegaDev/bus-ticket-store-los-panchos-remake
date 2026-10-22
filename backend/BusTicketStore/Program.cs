using BusTicketStore.Api.Middleware;
using BusTicketStore.Application;
using BusTicketStore.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// --- Composition root. No business logic, no domain rules, no fare amounts. ---

builder.Services.AddOpenApi();

// Each layer registers what it owns. This file never enumerates handlers, validators,
// repositories or DbContexts, because those lists change with every use case and the
// composition root is not the place to keep them in sync.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Minimal APIs have no automatic ProblemDetails. [ApiController] used to provide it in
// MVC, and here it has to be requested explicitly. This registers the
// IProblemDetailsService that GlobalExceptionHandler writes through.
builder.Services.AddProblemDetails();

// Registers the handler in the exception-handling pipeline. The middleware order below
// decides when that pipeline runs.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// Must be registered before the endpoints. Anything an endpoint throws and does not
// catch is routed here, which is the only reason a DomainException becomes 422 instead
// of 500.
app.UseExceptionHandler();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options
        .WithTitle("Bus Ticket Store - Los Panchos API")
        .WithTheme(ScalarTheme.DeepSpace));
}

app.UseHttpsRedirection();

app.Run();
