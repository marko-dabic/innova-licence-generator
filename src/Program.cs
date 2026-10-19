using System.Text.Json.Serialization;
using InnovaTest.LicenceGenerator;
using InnovaTest.LicenceGenerator.Configuration;
using InnovaTest.LicenceGenerator.Errors;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = ConfigurationDirectory.Find()
});

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<LicenceRequestExceptionHandler>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<LicenceIssuer>();
builder.Services.AddOptions<GeneratorOptions>()
    .Bind(builder.Configuration.GetRequiredSection("LicenceGenerator"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddSingleton<SigningKey>();

var app = builder.Build();

_ = app.Services.GetRequiredService<SigningKey>();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.MapControllers();
app.Run();
