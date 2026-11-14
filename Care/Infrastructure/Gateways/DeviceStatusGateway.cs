using System.Net;
using System.Net.Http.Headers;
using foll_care.Care.Application.Ports;
using foll_care.Care.Domain.Model.ValueObjects;

namespace foll_care.Care.Infrastructure.Gateways;

public class DeviceStatusGateway : IDeviceStatusGateway
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DeviceStatusGateway(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PatientDeviceStatusInfo?> GetByPatientIdAsync(long patientId)
    {
        var authorizationHeader = _httpContextAccessor.HttpContext?
            .Request.Headers["Authorization"].ToString();

        using var request = new HttpRequestMessage(HttpMethod.Get,
            $"internal/devices/patient/{patientId}/status");

        if (!string.IsNullOrWhiteSpace(authorizationHeader))
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(authorizationHeader);

        using var response = await _httpClient.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PatientDeviceStatusInfo>();
    }
}