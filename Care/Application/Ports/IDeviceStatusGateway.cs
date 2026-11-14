using foll_care.Care.Domain.Model.ValueObjects;

namespace foll_care.Care.Application.Ports;

public interface IDeviceStatusGateway
{
    Task<PatientDeviceStatusInfo?> GetByPatientIdAsync(long patientId);
}