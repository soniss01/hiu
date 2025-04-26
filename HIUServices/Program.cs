using HIUServices.DbContexts;
using HIUServices.Repostitories.Callbacks;
using HIUServices.Repostitories.Callbacks.Interfaces;
using HIUServices.Repostitories.Masters.Interfaces;
using HIUServices.Repostitories.Masters;
using HIUServices.Repostitories.Requests;
using HIUServices.Repostitories.Requests.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using HIUServices.Repositories.Masters;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<CallbackDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ConnStr"))); // PostgreSQL connection

builder.Services.AddDbContext<RequestDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ConnStr"))); // PostgreSQL connection

builder.Services.AddScoped<ICallbackService, CallbackService>();
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<IMasterService, MasterService>();
builder.Services.AddScoped<IAccessTokenService, AccessTokenService>();

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.Formatting = Formatting.Indented;
});

builder.Services.AddEndpointsApiExplorer();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOrigin", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "HIU APIs",
        Description = "APIs for managing ABHA HIUs Services."
    });
});

var app = builder.Build();

// Add logging
var loggerFactory = app.Services.GetService<ILoggerFactory>();
if (loggerFactory != null)
{
    loggerFactory.AddFile(builder.Configuration["Logging:LogFilePath"]);
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("../swagger/v1/swagger.json", "HIU");
});

app.UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
