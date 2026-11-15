namespace foll_care.Care.Domain.Model.ValueObjects;

public record PatientNotificationUserRecipient(long UserId, string FullName, string? PhoneNumber);
public record PatientNotificationEmergencyContact(long EmergencyContactId, string FullName, string PhoneNumber, string Relationship);
public record PatientNotificationRecipients(long PatientId,
    IReadOnlyCollection<PatientNotificationUserRecipient> UserRecipients,
    IReadOnlyCollection<PatientNotificationEmergencyContact> EmergencyContacts);
