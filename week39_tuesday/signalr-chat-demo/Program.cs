using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using signalr_chat.Data;
using signalr_chat.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSignalR();
builder.Services.AddControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("d5df9b30d5891d6e19c3eda79aef6fa0181cb5f0da195f2bbb54022c7d217b1b")) // Replace with your secret key
        };

        // Handle the JWT in the SignalR connection
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Check for the access token in the query string
                var accessToken = context.Request.Query["access_token"];

                // If found, set it
                if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/chathub"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddDbContext<ChatContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ChatDbConnection")));

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
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChatHub>("/chathub");
app.MapControllers();

app.Run();
