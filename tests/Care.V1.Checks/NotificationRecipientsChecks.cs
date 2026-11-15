using System.Net;
using System.Net.Http.Json;
using foll_care.Care.Application.Internal.QueryServices;
using foll_care.Care.Application.Ports;
using foll_care.Care.Domain.Model.Entities;
using foll_care.Care.Domain.Model.Enums;
using foll_care.Care.Domain.Model.Queries;
using foll_care.Care.Domain.Model.ValueObjects;
using foll_care.Care.Infrastructure.Persistence.EFC.Repositories;
using foll_care.Shared.Infrastructure.Persistence.EFC.Configuration;
using foll_care.Shared.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

static class NotificationRecipientsChecks
{
    public static async Task RunAsync(Action<bool, string> check)
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).UseSnakeCaseNamingConvention().Options);
        await db.Database.EnsureCreatedAsync();
        var patient = new Patient("87654321", "Ana", "Perez", new DateOnly(1950, 1, 1), 10, BloodType.Unknown, null, null);
        patient.AddCaregiver(10, 1);
        patient.AddCaregiver(20, 1);
        patient.AssignGuardShift(99, 10);
        patient.AddEmergencyContact("Contacto", "988888888", "Familiar");
        db.Patients.Add(patient);
        await db.SaveChangesAsync();
        var users = new Users();
        var query = new PatientQueryService(new PatientRepository(db), new PatientInvitationRepository(db), new RelationshipTypeRepository(db), users);
        var result = (await query.Handle(new GetPatientNotificationRecipientsQuery(patient.PatientId)))!;
        check(result.UserRecipients.Select(r => r.UserId).Order().SequenceEqual(new long[] { 10, 20 }),
            "notification recipients preserve guardian/caregiver rules and exclude unrelated current guardian");
        check(result.UserRecipients.Single(r => r.UserId == 10).PhoneNumber == "999999999" && result.EmergencyContacts.Single().PhoneNumber == "988888888",
            "notification recipients preserve user phones and emergency contacts");
        check(result.UserRecipients.Single(r => r.UserId == 20).FullName == "Usuario 20", "missing IAM user preserves monolith name fallback");
        check(await query.Handle(new GetPatientNotificationRecipientsQuery(9999)) is null, "unknown patient has no recipients");

        const string key = "test-background-query-key-with-at-least-32-characters";
        using var services = new ServiceCollection().AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["InternalApi:BackgroundQueryKey"] = key }).Build()).BuildServiceProvider();
        foreach (var supplied in new[] { "", "incorrect", key })
        {
            var context = new DefaultHttpContext { RequestServices = services };
            context.Request.Headers["X-Foll-Service-Key"] = supplied;
            var filter = new AuthorizationFilterContext(new ActionContext(context, new RouteData(), new ActionDescriptor()), []);
            new BackgroundQueryKeyAttribute().OnAuthorization(filter);
            check(supplied == key ? filter.Result is null : filter.Result is UnauthorizedResult, "recipient query validates its background credential");
        }

        foreach (var background in new[] { false, true })
        {
            var context = new DefaultHttpContext();
            if (background)
            {
                context.SetEndpoint(new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(new BackgroundQueryKeyAttribute()), "recipients"));
                context.Request.Headers["X-Foll-Service-Key"] = key;
            }
            else context.Request.Headers.Authorization = "Bearer user-token";
            using var client = new HttpClient(new Inspect(request =>
            {
                check(request.RequestUri!.AbsolutePath == (background ? "/internal/iam/users/10/notification-recipient" : "/api/iam/users/10"),
                    "Care selects the IAM contract for the request context");
                check(background ? request.Headers.Authorization is null && request.Headers.GetValues("X-Foll-Service-Key").Single() == key
                    : request.Headers.Authorization?.ToString() == "Bearer user-token" && !request.Headers.Contains("X-Foll-Service-Key"),
                    "Care keeps user JWT and background query credentials separate");
            })) { BaseAddress = new Uri("http://iam/") };
            await new UserInfoGateway(client, new HttpContextAccessor { HttpContext = context }).FindByIdAsync(10);
        }
    }

    private sealed class Users : IUserInfoGateway
    {
        public Task<UserInfo?> FindByIdAsync(long userId) => Task.FromResult<UserInfo?>(userId == 10 ? new(10, "test@example.com", "Ana", "Perez", "999999999") : null);
    }
    private sealed class Inspect(Action<HttpRequestMessage> inspect) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            inspect(request);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new UserInfo(10, "", "Ana", "Perez", "999999999")) });
        }
    }
}
