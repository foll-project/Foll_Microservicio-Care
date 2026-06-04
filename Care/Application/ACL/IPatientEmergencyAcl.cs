namespace foll_care.Care.Application.ACL;

public interface IPatientEmergencyAcl
{
    Task<PatientEmergencyAccessDto?> GetPatientEmergencyAccessByIdAsync(long patientId);
}
