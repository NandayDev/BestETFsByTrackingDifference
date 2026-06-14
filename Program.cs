using BestETFsByTD;
using BestETFsByTD.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using System.Globalization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
    .AddLocalization()
    .AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
    .AddScoped<IDataService, DataService>();

var host = builder.Build();

// leggi la cultura da localStorage via JS
var js = host.Services.GetRequiredService<IJSRuntime>();
var savedCulture = await js.InvokeAsync<string>("blazorCulture.get");

CultureInfo culture = CultureInfo.DefaultThreadCurrentCulture ?? new CultureInfo("en-GB");

if (!string.IsNullOrWhiteSpace(savedCulture))
{
    culture = new CultureInfo(savedCulture);
}

// imposta la cultura di default per il thread
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

await host.RunAsync();