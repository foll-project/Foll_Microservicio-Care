using foll_care.Care.Domain.Services;
using foll_care.Care.Domain.Model.Queries;
using foll_care.Care.Domain.Model.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;

namespace foll_care.Care.Interfaces.Internal;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("internal/patients")]
public class InternalPatientsController : ControllerBase
{
    private readonly IPatientQueryService _queryService;

    public InternalPatientsController(IPatientQueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpGet("{patientId:long}/device-access")]
    public async Task<IActionResult> GetDeviceAccess([FromRoute] long patientId)
    {
        var userId = GetUserIdOrThrow();
        var result = await _queryService.Handle(new GetPatientDeviceAccessQuery(userId, patientId));
        if (result is null) return NotFound(new { message = "Paciente no encontrado." });
        return Ok(result);
    }

    [HttpGet("{patientId:long}/emergency-access")]
    public async Task<IActionResult> GetEmergencyAccess([FromRoute] long patientId)
    {
        var userId = GetUserIdOrThrow();
        var result = await _queryService.Handle(new GetPatientEmergencyAccessQuery(userId, patientId));
        if (result is null) return NotFound(new { message = "Paciente no encontrado." });
        return Ok(result);
    }

    private long GetUserIdOrThrow()
    {
        var claim = User.FindFirst("userId")?.Value;
        if (!long.TryParse(claim, out var userId) || userId <= 0)
            throw new UnauthorizedAccessException("JWT inválido: userId no encontrado.");
        return userId;
    }
}
