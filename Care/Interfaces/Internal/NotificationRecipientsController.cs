using foll_care.Care.Domain.Model.Queries;
using foll_care.Care.Domain.Services;
using foll_care.Shared.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace foll_care.Care.Interfaces.Internal;

[ApiController]
[Route("internal/patients")]
public class NotificationRecipientsController(IPatientQueryService queries) : ControllerBase
{
    [HttpGet("{patientId:long}/notification-recipients")]
    [BackgroundQueryKey]
    public async Task<IActionResult> Get(long patientId)
    {
        var recipients = await queries.Handle(new GetPatientNotificationRecipientsQuery(patientId));
        return recipients is null ? NotFound() : Ok(recipients);
    }
}
