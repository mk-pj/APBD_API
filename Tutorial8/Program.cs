using Tutorial8.Middlewares;
using Tutorial8.Repositories;
using Tutorial8.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<ITripsService, TripsService>();
builder.Services.AddScoped<IClientService, ClientService>();

builder.Services.AddScoped<ITripsRepository, TripsRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseGlobalExceptionHandler();

app.UseAuthorization();

app.MapControllers();

app.Run();