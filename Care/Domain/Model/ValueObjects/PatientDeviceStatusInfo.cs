namespace foll_care.Care.Domain.Model.ValueObjects;

public record PatientDeviceStatusInfo(
    long DeviceId,
    long? AssignedPatientId,
    string Status,
    string? ConnectivityStatus,
    short? CurrentBatteryLevel,
    bool? IsCharging,
    DateTime? LastHeartbeatAt,
    DateTime? MonitoringStartedAt,
    DateTime? LastConnectivityChangeAt,
    bool IsOnline,
    bool IsLowBattery,
    string FirmwareVersion);
