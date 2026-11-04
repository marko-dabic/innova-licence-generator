using InnovaTest.LicenceGenerator.Configuration;
using InnovaTest.LicenceGenerator.Persistence;
using InnovaTest.LicenceGenerator.Security;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = ConfigurationDirectory.Find()
});

using var environmentFileProvider = new PhysicalFileProvider(
    builder.Environment.ContentRootPath, ExclusionFilters.None);
builder.Configuration
    .AddIniFile(environmentFileProvider, ".env", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

builder.Services.AddLicenceGenerator(builder.Configuration);

var app = builder.Build();

_ = app.Services.GetRequiredService<SigningKey>();
await app.Services.GetRequiredService<LicenceDatabaseInitializer>()
    .InitializeAsync(app.Lifetime.ApplicationStopping);

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
await app.RunAsync();
