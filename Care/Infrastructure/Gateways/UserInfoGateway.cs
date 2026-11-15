using System.Net;
using System.Net.Http.Headers;
using foll_care.Care.Application.Ports;
using foll_care.Care.Domain.Model.ValueObjects;

public class UserInfoGateway(HttpClient client, IHttpContextAccessor contexts) : IUserInfoGateway
{
    public async Task<UserInfo?> FindByIdAsync(long userId)
    {
        var context = contexts.HttpContext;
        var authorization = context?.Request.Headers.Authorization.ToString();
        var backgroundKey = context?.Request.Headers["X-Foll-Service-Key"].ToString();
        var background = context?.GetEndpoint()?.Metadata.GetMetadata<foll_care.Shared.Infrastructure.Security.BackgroundQueryKeyAttribute>() is not null;
        if (!background && string.IsNullOrWhiteSpace(authorization))
            throw new InvalidOperationException("La consulta de usuario requiere el JWT del solicitante.");
        using var request = new HttpRequestMessage(HttpMethod.Get, background
            ? $"internal/iam/users/{userId}/notification-recipient" : $"api/iam/users/{userId}");
        if (background) request.Headers.Add("X-Foll-Service-Key", backgroundKey);
        else request.Headers.Authorization = AuthenticationHeaderValue.Parse(authorization!);
        using var response = await client.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        UserInfo? user;
        if (background)
        {
            var recipient = await response.Content.ReadFromJsonAsync<NotificationRecipient>();
            user = recipient is null ? null : new UserInfo(recipient.UserId, "", recipient.FirstName, recipient.LastName, recipient.PhoneNumber);
        }
        else user = await response.Content.ReadFromJsonAsync<UserInfo>();
        if (user is not null && user.UserId != userId) throw new InvalidOperationException("IAM devolvio otro usuario.");
        return user;
    }

    private sealed record NotificationRecipient(long UserId, string FirstName, string LastName, string? PhoneNumber);
}
