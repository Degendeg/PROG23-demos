using signalr_chat.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSignalR();

// Change this to what ports you have in your launchSettings.json
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5274); // HTTP
    options.ListenAnyIP(7152, listenOptions =>
    {
        listenOptions.UseHttps(); // HTTPS
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseDefaultFiles(); // Enables default files (e.g., index.html)
app.UseStaticFiles(); // Allows serving static files from wwwroot
app.UseRouting();
app.UseAuthorization();

app.MapHub<ChatHub>("/chathub");

app.Run();
