using backend.Hubs;
using backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<CircuitStateService>();
builder.Services.AddSignalR();
builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:4173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

var app = builder.Build();

app.UseCors("AllowAll");
app.UseRouting();

// Map SignalR Hub
app.MapHub<CircuitHub>("/circuithub");
app.MapControllers();

app.Run();
