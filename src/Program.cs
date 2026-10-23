using System.Text.Json.Serialization;
using InnovaTest.LicenceGenerator.Swagger;
using InnovaTest.LicenceGenerator.Services;
using InnovaTest.LicenceGenerator.Configuration;
using InnovaTest.LicenceGenerator.Errors;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = ConfigurationDirectory.Find()
});

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow);
builder.Services.AddSwaggerGen(options =>
    options.SchemaFilter<LicenceRequestExampleFilter>());
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<LicenceRequestExceptionHandler>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ILicenceService, LicenceService>();
builder.Services.AddOptions<GeneratorOptions>()
    .Bind(builder.Configuration.GetRequiredSection("LicenceGenerator"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddSingleton<SigningKey>();

var app = builder.Build();

_ = app.Services.GetRequiredService<SigningKey>();
app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();
