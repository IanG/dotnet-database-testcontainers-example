using Microsoft.AspNetCore.HttpLogging;
using Serilog;
using Serilog.Core;
using TestingContainersExample.Common.Data;
using TestingContainersExample.Common.Services;

var builder = WebApplication.CreateBuilder(args);

// Set up Logging with SeriLog
Logger logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddSerilog(logger);

// Add DbContexts
builder.Services.AddDbContexts(builder.Configuration);

// Add Services
builder.Services.AddServices(builder.Configuration);

// Force all routes and query strings to be lowercase
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

builder.Services.AddHttpLogging(o =>
{
    o.LoggingFields = HttpLoggingFields.RequestPath
                      | HttpLoggingFields.RequestHeaders
                      | HttpLoggingFields.ResponseHeaders
                      | HttpLoggingFields.RequestBody
                      | HttpLoggingFields.ResponseBody
                      | HttpLoggingFields.ResponseStatusCode;
    
    // Limit the size of the logged request body to 4 KB
    o.RequestBodyLogLimit = 4096; // 4KB
    
    // Limit the size of the logged response body to 4 KB
    o.ResponseBodyLogLimit = 4096; // 4KB

    // Filter by media types (log JSON but exclude plain text)
    o.MediaTypeOptions.AddText("application/json");
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");

app.UseHttpsRedirection();

app.UseHttpLogging();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }