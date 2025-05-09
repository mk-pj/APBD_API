using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
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

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Tutorial8",
        Version = "v1",
        Description = "Tutorial8 APBD homework",
        Contact = new OpenApiContact()
        {
            Name = "Tutorial8 API Support",
            Url = new Uri("https://github.com/mk-pj/APBD_API")
        },
        License = new OpenApiLicense()
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tutorial8");
    c.DocExpansion(DocExpansion.List);
    c.DefaultModelExpandDepth(0);
    c.DisplayRequestDuration();
    c.EnableFilter();
});

app.UseGlobalExceptionHandler();

app.UseAuthorization();

app.MapControllers();

app.Run();