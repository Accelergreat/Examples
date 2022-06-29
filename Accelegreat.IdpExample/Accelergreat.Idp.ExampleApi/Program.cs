using Accelergreat.Idp.Database.Contexts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Jwt:Issuer"];
        options.Audience = builder.Configuration["Jwt:Audience"];
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuerSigningKey = false,
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateAudience = true
        };
        options.Events = new JwtBearerEvents()
        {

            OnMessageReceived = (context) =>
            {
                var principal = context.Principal;
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = (context) =>
            {
                var exception = context.Exception;
                return Task.CompletedTask;
            },
            OnTokenValidated = (context) =>
            {
                var principal = context.Principal;
                context.Success();
                return Task.CompletedTask;

            },
            OnForbidden = (context) =>
            {
                var principal = context.Principal;
                return Task.CompletedTask;
            }
        };
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddDbContext<BloggingContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("BloggingContext"))
    );
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app
    .UseAuthentication()
    .UseAuthorization();

app.MapControllers();

app.Run();
