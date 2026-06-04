namespace foll_care.Care.Interfaces.REST.Resources;

public record AddEmergencyContactResource(string FullName, string PhoneNumber, object? Relationship);
