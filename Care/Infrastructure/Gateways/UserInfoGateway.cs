using foll_care.Care.Application.Ports;
using foll_care.Care.Domain.Model.ValueObjects;
using System.Net.Http.Headers;

public class UserInfoGateway : IUserInfoGateway
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserInfoGateway(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<UserInfo?> FindByIdAsync(long userId)
    {
        var authorizationHeader =
            _httpContextAccessor.HttpContext?
                .Request.Headers["Authorization"]
                .ToString();

        if (!string.IsNullOrWhiteSpace(authorizationHeader))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                AuthenticationHeaderValue.Parse(authorizationHeader);
        }

        return await _httpClient.GetFromJsonAsync<UserInfo>(
            $"api/iam/users/{userId}");
    }
}