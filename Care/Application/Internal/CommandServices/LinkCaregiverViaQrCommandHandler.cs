using foll_care.Care.Application.Ports;
using foll_care.Care.Domain.Model.Commands;
using foll_care.Care.Domain.Repositories;
using foll_care.Shared.Domain.Repositories;
using MediatR;

namespace foll_care.Care.Application.Internal.CommandServices;

public class LinkCaregiverViaQrCommandHandler : IRequestHandler<LinkCaregiverViaQrCommand>
{
    private readonly IPatientRepository _patientRepository;
    private readonly IUserInfoGateway _userInfoGateway;
    private readonly IUnitOfWork _unitOfWork;

    public LinkCaregiverViaQrCommandHandler(
        IPatientRepository patientRepository,
        IUserInfoGateway userInfoGateway,
        IUnitOfWork unitOfWork)
    {
        _patientRepository = patientRepository;
        _userInfoGateway = userInfoGateway;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(LinkCaregiverViaQrCommand request, CancellationToken cancellationToken)
    {
        var patient = await _patientRepository.FindByIdAsync(request.PatientId);
        if (patient is null) throw new InvalidOperationException("Paciente no encontrado.");

        var caregiver = await _userInfoGateway.FindByIdAsync(request.CaregiverId);
        if (caregiver is null) throw new InvalidOperationException("Cuidador no encontrado.");

        if (patient.Caregivers.Any(c => c.UserId == request.CaregiverId))
            throw new InvalidOperationException("El cuidador ya está vinculado al paciente.");

        // Fast-Track QR link uses default relationship type (1 = Familiar/Cuidador).
        patient.AddCaregiver(request.CaregiverId, 1);

        _patientRepository.Update(patient);
        await _unitOfWork.CompleteAsync();
    }
}
