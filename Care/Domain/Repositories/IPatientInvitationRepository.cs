using foll_care.Care.Domain.Model.Entities;
using foll_care.Shared.Domain.Repositories;

namespace foll_care.Care.Domain.Repositories;

public interface IPatientInvitationRepository : IBaseRepository<PatientInvitation>
{
    Task<IEnumerable<PatientInvitation>> ListByPatientIdAsync(long patientId);
}
