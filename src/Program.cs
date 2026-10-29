using InnovaTest.LicenceGenerator.Configuration;
using InnovaTest.LicenceGenerator.Errors;
using InnovaTest.LicenceGenerator.Features.Licences.Activate;
using InnovaTest.LicenceGenerator.Features.Licences;
using InnovaTest.LicenceGenerator.Persistence;
using InnovaTest.LicenceGenerator.Security;
using InnovaTest.LicenceGenerator.Swagger;
using LicenceGeneratorHandler = InnovaTest.LicenceGenerator.Features.Licences.Generate.LicenceGenerator;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = ConfigurationDirectory.Find()
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
    options.JsonSerializerOptions.AllowOutOfOrderMetadataProperties = true;
});
builder.Services.AddSwaggerGen(options =>
{
    options.UseOneOfForPolymorphism();
    options.SelectDiscriminatorNameUsing(_ => "licenceType");
    options.SelectDiscriminatorValueUsing(type => type.Name.Replace("LicenceDto", ""));
    options.SchemaFilter<LicenceRequestExampleFilter>();
});
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<LicenceRequestExceptionHandler>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<LicenceActivationStore>();
builder.Services.AddSingleton<LicenceTokenCodec>();
builder.Services.AddSingleton<LicenceGeneratorHandler>();
builder.Services.AddSingleton<LicenceActivator>();
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
