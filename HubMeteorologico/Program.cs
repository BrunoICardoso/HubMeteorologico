using HubMeteorologico.Domain.Appsettings;
using HubMeteorologico.Infrastructure.Repository.Settings.Interface;
using HubMeteorologico.Infrastructure.Repository.Settings;
using Serilog;
using Serilog.Events;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using HubMeteorologico.Infrastructure.Messaging;
using Npgsql;
using FluentValidation.AspNetCore;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using MotoRental.API.ConfigController;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

#region Serilog

Log.Logger = new LoggerConfiguration()
.MinimumLevel.Debug()
.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
.Enrich.FromLogContext()
.WriteTo.Console()
.CreateLogger();


builder.Host.UseSerilog()
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddEnvironmentVariables();
            });
#endregion


#region Configuration of Routes, Controllers and MVC
builder.Services.AddMvc()
 .AddJsonOptions(options =>
 {
     options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
 });

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
#endregion

#region Configuration RabbitMQ
builder.Services.Configure<RabbitMQOptions>(builder.Configuration.GetSection("RabbitMQ"));

builder.Services.AddSingleton<RabbitMQClient>(sp =>
{
    var rabbitMQOptions = sp.GetRequiredService<IOptions<RabbitMQOptions>>().Value;
    return new RabbitMQClient(rabbitMQOptions.HostName, rabbitMQOptions.UserName, rabbitMQOptions.Password);
});

#endregion

#region Configuração Context DB

builder.Services.AddOptions();
builder.Services.Configure<DataSettings>(builder.Configuration.GetSection("ConnectionStrings"));

builder.Services.AddSingleton<NpgsqlDataSource>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<DataSettings>>().Value;

    if (string.IsNullOrWhiteSpace(settings.ContextBase))
        throw new InvalidOperationException("Connection string (ConnectionStrings:ContextBase) não configurada.");

    var dataSourceBuilder = new NpgsqlDataSourceBuilder(settings.ContextBase);

    dataSourceBuilder.UseNetTopologySuite(
        geographyAsDefault: true
    );

    return dataSourceBuilder.Build();
});

builder.Services.AddScoped<IDatabaseFactory, DatabaseFactory>();
builder.Services.AddScoped<IDbSession, DbSession>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


#endregion

#region FluentValidation

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.Configure<ApiBehaviorOptions>(opt =>
{
    opt.SuppressModelStateInvalidFilter = true;
});


#endregion

#region Services Internal

#endregion

#region Repositories

#endregion

#region Swagger
builder.Services.AddSwaggerGen(s =>
{
    s.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "API HubMeteorologico ",
        Description = "",
        Contact = new OpenApiContact
        {
            Name = "HubMeteorologico",
            Email = "HubMeteorologico@gmail.com",

        },
        License = new OpenApiLicense
        {
            Name = "MIT",
        }

    });

    s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    s.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

    s.OperationFilter<SwaggerDefaultValues>();

    // integrate xml comments
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    s.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    // allows same class names in different namespaces
    s.CustomSchemaIds(x => x.FullName);

    s.SchemaFilter<FluentValidationSwaggerSchemaFilter>();

});
#endregion


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
