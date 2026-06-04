using foll_care.Care.Domain.Model.Enums;

namespace foll_care.Care.Domain.Model.Commands;

public record UpdatePatientCommand(
    long ActorUserId,
    long PatientId,
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    BloodType BloodType,
    Dictionary<string, string>? MedicalConditions,
    Dictionary<string, string>? Medications); //SIGUE LA SECUENCIA
