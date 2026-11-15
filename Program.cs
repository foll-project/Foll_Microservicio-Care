using foll_care.Care.Infrastructure.Gateways;
// ACL layer removed: endpoints moved to internal controllers
using System.Text;
using foll_care.Care.Application.Internal.CommandServices;
using foll_care.Care.Application.Internal.QueryServices;
using foll_care.Care.Application.OutboundServices;
using foll_care.Care.Domain.Repositories;
using foll_care.Care.Domain.Services;
using foll_care.Care.Infrastructure.Persistence.EFC.Repositories;
using foll_care.Shared.Domain.Repositories;
using foll_care.Shared.Infrastructure.Persistence.EFC.Configuration;
using foll_care.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using foll_care.Shared.Infrastructure.Swagger;
using foll_care.Care.Application.Ports;


var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString)
        .UseSnakeCaseNamingConvention();
});

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Foll Care Microservice",
        Version = "v1",
        Description = "Microservicio de Foll : Care. Centralización de la gestión de adultos mayores."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pegue SOLO el JWT, sin escribir Bearer."
    });

    options.AddSecurityDefinition("BackgroundQueryKey", new OpenApiSecurityScheme
    {
        Name = "X-Foll-Service-Key", Type = SecuritySchemeType.ApiKey, In = ParameterLocation.Header,
        Description = "Consulta de contexto sin sesión de usuario (worker/enlace público)."
    });
    options.OperationFilter<AuthorizeOperationFilter>();
});


builder.Services.AddHttpContextAccessor();


builder.Services.AddHttpClient<IDeviceStatusGateway, DeviceStatusGateway>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Services:DeviceManagment:BaseUrl"]!);
});

// ================== MICROSERVICIO IAM

builder.Services.AddHttpClient<IUserInfoGateway, UserInfoGateway>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Services:Iam:BaseUrl"]!);
})
    .RedactLoggedHeaders(["Authorization", "X-Foll-Service-Key"])
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });



// ================== MICROSERVICIO IAM
var jwtSecret = builder.Configuration["JwtSettings:Secret"];

if (!string.IsNullOrEmpty(jwtSecret))
{
    var key = Encoding.ASCII.GetBytes(jwtSecret);

    builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };
        });
}

builder.Services.AddAuthorization();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Former ACL registrations removed. Access endpoints are exposed
// via internal controllers and handled by IPatientQueryService.

builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IPatientInvitationRepository, PatientInvitationRepository>();
builder.Services.AddScoped<IRelationshipTypeRepository, RelationshipTypeRepository>();

builder.Services.AddScoped<IPatientCommandService, PatientCommandService>();
builder.Services.AddScoped<IPatientQueryService, PatientQueryService>();
builder.Services.AddScoped<IRelationshipTypeQueryService, RelationshipTypeQueryService>();

builder.Services.AddScoped<ICareEventService, foll_care.Care.Infrastructure.Messaging.Outbox.OutboxCareEventService>();
builder.Services.AddOptions<foll_care.Care.Infrastructure.Messaging.Configuration.IntegrationMessagingOptions>()
    .Bind(builder.Configuration.GetSection("IntegrationMessaging"))
    .Validate(o => o.PollIntervalSeconds > 0 && o.BatchSize > 0 && o.BatchSize <= 100,
        "PollIntervalSeconds debe ser positivo y BatchSize debe estar entre 1 y 100.")
    .ValidateOnStart();
builder.Services.AddHostedService<foll_care.Care.Infrastructure.Messaging.Outbox.OutboxPublisherBackgroundService>();

builder.Services.AddOptions<foll_care.Care.Infrastructure.Messaging.Configuration.RabbitMqOptions>()
    .Bind(builder.Configuration.GetSection("RabbitMq"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.HostName) && o.Port > 0 && o.Port <= 65535 &&
        !string.IsNullOrWhiteSpace(o.UserName) && !string.IsNullOrWhiteSpace(o.Password) &&
        !string.IsNullOrWhiteSpace(o.VirtualHost) && !string.IsNullOrWhiteSpace(o.ExchangeName) &&
        !string.IsNullOrWhiteSpace(o.QueueName) && o.PublishTimeoutSeconds > 0 && o.PublishTimeoutSeconds <= 120,
        "Configuracion RabbitMq incompleta o invalida.")
    .ValidateOnStart();
builder.Services.AddSingleton<foll_care.Care.Infrastructure.Messaging.RabbitMq.RabbitMqConnection>(sp => new(
    sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<foll_care.Care.Infrastructure.Messaging.Configuration.RabbitMqOptions>>(),
    new[] { foll_care.Care.Application.IntegrationEvents.InvitationChangedIntegrationEvent.CreatedEventType, foll_care.Care.Application.IntegrationEvents.InvitationChangedIntegrationEvent.AcceptedEventType, foll_care.Care.Application.IntegrationEvents.InvitationChangedIntegrationEvent.RejectedEventType },
    sp.GetRequiredService<ILogger<foll_care.Care.Infrastructure.Messaging.RabbitMq.RabbitMqConnection>>()));
builder.Services.AddSingleton<foll_care.Care.Application.OutboundServices.IIntegrationEventPublisher,
    foll_care.Care.Infrastructure.Messaging.RabbitMq.RabbitMqIntegrationEventPublisher>();

builder.Services.AddSingleton<foll_care.Care.Infrastructure.Messaging.RabbitMq.RabbitMqIntegrationEventPublisher>();
foll_care.Shared.Infrastructure.Messaging.IntegrationConsumerRegistration.AddIntegrationConsumer<foll_care.Shared.Infrastructure.Messaging.AccountDeletionRequestedHandler>(builder.Services, builder.Configuration, "care.account-deletion-requested.v1", foll_care.Shared.Infrastructure.Messaging.AccountDeletionRequested.EventType);
var app = builder.Build();

var applyMigrations = builder.Configuration.GetValue<bool>("ApplyMigrationsOnStartup", true);

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();

    if (applyMigrations)
    {
        context.Database.Migrate();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else if (builder.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
