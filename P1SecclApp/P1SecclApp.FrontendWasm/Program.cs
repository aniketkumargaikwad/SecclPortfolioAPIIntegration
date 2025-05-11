// P1SecclApp.FrontendWasm/Program.cs
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using P1SecclApp.FrontendWasm; // For root components like App
using P1SecclApp.FrontendWasm.Services; // Our new service

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app"); // Sets the root component of the app
builder.RootComponents.Add<HeadOutlet>("head::after"); // Handles <head> content

// **** OUR CHANGES START HERE ****

// Configure HttpClient to point to your BackendApi
// The port number (e.g., 7001) MUST match where your BackendApi is running.
// Check P1SecclApp.BackendApi/Properties/launchSettings.json for its HTTPS port.
// It's common to have different ports for API and Frontend in development.
// Example: BackendApi on https://localhost:7001, FrontendWasm on https://localhost:7123
string backendApiUrl = builder.HostEnvironment.BaseAddress; // Default if not specified
if (builder.HostEnvironment.IsDevelopment())
{
    // Replace with your actual BackendApi HTTPS URL if different from the Blazor app's base
    // Typically, BackendApi runs on a different port.
    // Find your BackendApi's HTTPS port from its Properties/launchSettings.json
    backendApiUrl = "https://localhost:7139"; // <<< !!! UPDATE THIS PORT !!! (Check your BackendApi launchSettings.json https profile applicationUrl)
}


builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(backendApiUrl) });

// Register our PortfolioDataService
builder.Services.AddScoped<IPortfolioDataService, PortfolioDataService>();

// **** OUR CHANGES END HERE ****

await builder.Build().RunAsync();