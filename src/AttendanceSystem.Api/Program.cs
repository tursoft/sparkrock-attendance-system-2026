using AttendanceSystem.Infrastructure;
using AttendanceSystem.Api.Endpoints;
using AttendanceSystem.Api.ErrorHandling;
using AttendanceSystem.Api.Hosting;
using AttendanceSystem.Application;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddJsonStdoutLogging();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddErrorHandling();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SupportNonNullableReferenceTypes();
    options.NonNullableReferenceTypesAsRequired();
});
// Binding failures throw in every environment, so GlobalExceptionHandler can name the failing field.
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);
builder.Services.AddHealthChecks();
builder.Services.AddRequestLogging();
builder.Services.AddConfiguredCors();

var app = builder.Build();

app.UseHttpLogging();
app.UseExceptionHandler();
app.UseStatusCodePages();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors();

app.UseSwagger(options => options.RouteTemplate = "openapi/{documentName}.json");
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Attendance API v1"));
app.MapHealthChecks("/health");
app.MapApiEndpoints();

app.Run();

public partial class Program;
