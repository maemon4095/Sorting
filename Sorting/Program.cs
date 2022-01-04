using MessagePipe;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Sorting;
using Sorting.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var services = builder.Services;

services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
        .AddScoped<DimensionService>()
        .AddScoped<LocalStorageService>()
        .AddMessagePipe();
var sp = services.BuildServiceProvider();
GlobalMessagePipe.SetProvider(sp);

await builder.Build().RunAsync();
