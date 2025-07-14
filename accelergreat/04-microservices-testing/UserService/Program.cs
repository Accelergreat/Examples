using UserService.Models;
using UserService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HTTP client for inter-service communication
builder.Services.AddHttpClient();

// Register services
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService.Services.UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run(); 