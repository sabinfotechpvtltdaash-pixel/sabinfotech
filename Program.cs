using Erpweb.Components;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Razor Components & Interactive Server Components
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// 2. Configure SignalR Hub limits specifically for Blazor Server
builder.Services.AddServerSideBlazor(options =>
{
    // Increases JS Interop timeout to allow processing large payloads
    options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(4);
    options.DetailedErrors = builder.Environment.IsDevelopment();
})
.AddHubOptions(options =>
{
    // Expands inbound SignalR message size limit to 32 MB for image transfers
    options.MaximumReceiveMessageSize = 32 * 1024 * 1024; // 32MB
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});
builder.Services.AddControllers(); // Add this line
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();