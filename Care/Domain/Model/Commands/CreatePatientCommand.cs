using foll_care.Care.Domain.Model.Enums;

namespace foll_care.Care.Domain.Model.Commands;

public record CreatePatientCommand(
    long ActorUserId,
    string Dni,
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    short RelationshipTypeId,
    BloodType BloodType,
    Dictionary<string, string>? MedicalConditions,
    Dictionary<string, string>? Medications); // <-- NUEVO
