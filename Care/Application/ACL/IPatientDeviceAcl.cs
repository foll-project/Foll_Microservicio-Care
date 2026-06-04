namespace foll_care.Care.Application.ACL;

public interface IPatientDeviceAcl
{
    Task<PatientDeviceAccessDto?> GetPatientDeviceAccessByIdAsync(long patientId);
}
