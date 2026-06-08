using System.Collections.Generic;

namespace foll_care.Care.Domain.Model.ValueObjects;

public record PatientEmergencyAccess(
    long PatientId,
    long OfficialGuardianUserId,
    long? CurrentGuardianUserId,
    IReadOnlyCollection<long> CaregiverUserIds);
