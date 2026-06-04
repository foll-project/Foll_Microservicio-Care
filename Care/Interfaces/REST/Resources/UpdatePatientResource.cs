using foll_care.Care.Domain.Model.Enums;

namespace foll_care.Care.Interfaces.REST.Resources;

public record UpdatePatientResource(
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    BloodType BloodType,
    Dictionary<string, string>? MedicalConditions,
    Dictionary<string, string>? Medications);//NUEVO
