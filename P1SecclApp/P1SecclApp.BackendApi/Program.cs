// P1SecclApp.BackendApi/Program.cs
using P1SecclApp.Core.Services; // Our core services
using P1SecclApp.Core.Models;  // Our core models
using System.Collections.Generic; // For List
using Microsoft.AspNetCore.Mvc; // For FromQuery

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// **** OUR CHANGES START HERE ****

// 1. Configure HttpClient for SECCL API calls
builder.Services.AddHttpClient<ISecclApiService, SecclApiService>();

// 2. Register our custom services for dependency injection
builder.Services.AddScoped<ISecclApiService, SecclApiService>();
builder.Services.AddScoped<IPortfolioAggregationService, PortfolioAggregationService>();

// 3. Add Configuration so SecclApiService can read appsettings.json
// This is often added by default, but good to be explicit.
// IConfiguration is already available via builder.Configuration

// 4. Add CORS (Cross-Origin Resource Sharing) services
// This is VERY IMPORTANT for Blazor WASM to call the API from a different "origin" (domain/port)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp",
        policy =>
        {
            policy.WithOrigins("https://localhost:7123", "http://localhost:5123") // FrontendWasm default ports (check yours!)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});
// **** OUR CHANGES END HERE ****

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp", // This is the policy name
        policy =>
        {
            // OLD/PREVIOUS EXAMPLE:
            // policy.WithOrigins("https://localhost:7123", "http://localhost:5123")
            //       .AllowAnyHeader()
            //       .AllowAnyMethod();

            // **** UPDATE THIS LINE ****
            // Add the origin for your Blazor WASM app (https://localhost:7240)
            // If your Blazor app also has an HTTP URL (e.g., http://localhost:XXXX), add that too.
            policy.WithOrigins("https://localhost:7240") // <<< ADD/UPDATE THIS
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

app.UseCors("AllowBlazorApp");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// **** OUR CHANGES START HERE ****
// 5. Use CORS - IMPORTANT: This must be before UseAuthorization and MapEndpoints/MapControllers
app.UseCors("AllowBlazorApp");
// **** OUR CHANGES END HERE ****


// Define Minimal API Endpoints
// Hint: You may need to adjust clientPortfolioIds based on available test data from SECCL API
// The PortfolioAggregationService.DefaultPortfolioIds are placeholders.

app.MapGet("/api/portfolio/aggregated-total",
    async (IPortfolioAggregationService service, [FromQuery] string[]? ids) =>
    {
        var portfolioIds = ids?.ToList() ?? PortfolioAggregationService.DefaultPortfolioIds;
        if (!portfolioIds.Any())
        {
            return Results.BadRequest("Please provide at least one portfolio ID using 'ids' query parameter.");
        }

        var result = await service.GetAggregatedTotalValueAsync(portfolioIds);
        return result != null ? Results.Ok(result) : Results.NotFound("Could not aggregate total value or portfolios not found.");
    })
.WithName("GetAggregatedPortfolioTotal")
.WithOpenApi(); // Ensure it's included in Swagger

app.MapGet("/api/portfolio/aggregated-by-accounttype",
    async (IPortfolioAggregationService service, [FromQuery] string[]? ids) =>
    {
        var portfolioIds = ids?.ToList() ?? PortfolioAggregationService.DefaultPortfolioIds;
        if (!portfolioIds.Any())
        {
            return Results.BadRequest("Please provide at least one portfolio ID using 'ids' query parameter.");
        }

        var result = await service.GetAggregatedTotalsByAccountTypeAsync(portfolioIds);
        return result != null ? Results.Ok(result) : Results.NotFound("Could not aggregate by account type or portfolios not found.");
    })
.WithName("GetAggregatedPortfolioByAccountType")
.WithOpenApi(); // Ensure it's included in Swagger


app.Run();