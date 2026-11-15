namespace foll_care.Care.Infrastructure.Messaging.Configuration;

public class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string VirtualHost { get; set; } = "/";
    public string ExchangeName { get; set; } = "foll.integration";
    public string QueueName { get; set; } = "notificationcommunication.invitation-events.v1";
    public int PublishTimeoutSeconds { get; set; } = 10;
    public string[] CompletionQueueNames { get; set; } =
        ["iam.care-account-deletion-completed.v1", "devices.care-account-deletion-completed.v1", "emergency.care-account-deletion-completed.v1"];
}
