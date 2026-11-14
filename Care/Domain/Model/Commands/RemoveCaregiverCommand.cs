using MediatR;

namespace foll_care.Care.Domain.Model.Commands;

public record RemoveCaregiverCommand(long CurrentUserId, long PatientId, long CaregiverId) : IRequest;
