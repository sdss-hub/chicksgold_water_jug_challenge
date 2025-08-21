using WaterJugChallenge.Services;
using FluentValidation;
using WaterJugChallenge.Models;
using WaterJugChallenge.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Water Jug Challenge API", 
        Version = "v1",
        Description = "RESTful API to solve the classic Water Jug Riddle using optimal BFS algorithm"
    });
    
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddMemoryCache();

builder.Services.AddScoped<IWaterJugSolver, WaterJugSolver>();
builder.Services.AddScoped<IValidator<WaterJugRequest>, WaterJugRequestValidator>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Water Jug Challenge API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "Water Jug Challenge API";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => new
{
    Status = "Healthy",
    Timestamp = DateTime.UtcNow,
    Version = "1.0.0",
    Environment = app.Environment.EnvironmentName
});

app.MapGet("/", () => new
{
    Message = "Water Jug Challenge API",
    Documentation = "/swagger",
    Health = "/health",
    Version = "1.0.0"
});

app.Run();

public partial class Program { }