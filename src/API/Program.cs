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

builder.Services.AddHttpLogging(o => { });

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }