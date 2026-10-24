using AttendanceSystem.Api.Endpoints;
using AttendanceSystem.Api.ErrorHandling;
using AttendanceSystem.Api.Hosting;
using AttendanceSystem.Application;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddJsonStdoutLogging();

builder.Services.AddApplication();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddErrorHandling();
builder.Services.AddValidation();
builder.Services.AddOpenApi();
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

app.MapOpenApi();
app.MapHealthChecks("/health");
app.MapApiEndpoints();

app.Run();

public partial class Program;
