using MediatR;

namespace foll_care.Care.Domain.Model.Commands;

public record LinkCaregiverViaQrCommand(long PatientId, long CaregiverId) : IRequest;
