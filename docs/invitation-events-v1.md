# Eventos de invitaciones — V1

Esta versión sigue el alcance de IAM V1: registra eventos de integración en un outbox transaccional y prepara su publicación mediante un puerto de Application. No incluye conexión a RabbitMQ, consumidores, inbox ni eliminación de pacientes por solicitudes de IAM. No envía aún avisos reales a NotificationCommunication o al frontend.

## Funcionalidades

- Crear invitación guarda `care.invitation-created.v1`, dirigido al cuidador principal.
- Aceptar invitación guarda `care.invitation-accepted.v1`, dirigido al solicitante, junto con la relación del nuevo cuidador.
- Rechazar invitación guarda `care.invitation-rejected.v1`, dirigido al solicitante.

Las rutas, respuestas y reglas existentes se conservan. Repetir una aceptación o rechazo sigue siendo inválido y no genera otro evento. Las solicitudes de creación distintas pueden crear invitaciones distintas, como antes; el outbox no añade idempotencia a la API.

## Organización

- `Care/Application/IntegrationEvents/InvitationChangedIntegrationEvent.cs`: contrato JSON común para las tres variantes y sus tipos estables.
- `Care/Application/OutboundServices/ICareEventService.cs`: puerto para registrar eventos de invitaciones.
- `Care/Application/OutboundServices/IIntegrationEventPublisher.cs`: puerto de publicación al futuro transporte.
- `Care/Application/Internal/CommandServices/PatientCommandService.cs`: registra el evento como parte de la operación.
- `Care/Infrastructure/Messaging/Configuration/IntegrationMessagingOptions.cs`: activación, intervalo y tamaño de lote.
- `Care/Infrastructure/Messaging/Outbox/IntegrationOutboxMessage.cs`: persistencia del evento y sus intentos.
- `Care/Infrastructure/Messaging/Outbox/OutboxCareEventService.cs`: registra el mensaje en el mismo AppDbContext, sin guardarlo por separado.
- `Care/Infrastructure/Messaging/Outbox/OutboxPublisherBackgroundService.cs`: reclama y publica mensajes pendientes.
- `Care/Infrastructure/Persistence/EFC/Configuration/IntegrationOutboxMessageConfiguration.cs`: mapeo de `care.integration_outbox_messages`.
- `Shared/Domain/Repositories/IUnitOfWork.cs` y su implementación: transacción explícita para la creación con ID generado.
- `Shared/Infrastructure/Persistence/EFC/Configuration/AppDbContext.cs`: DbSet del outbox.
- `Migrations/*_AddInvitationIntegrationOutbox*`: migración y snapshot.
- `Program.cs` y `appsettings.json`: DI y opciones.
- `tests/Care.V1.Checks`: comprobaciones ejecutables.

Domain conserva las reglas de las invitaciones. Interfaces conserva sus endpoints; no necesita clases nuevas en esta etapa porque no se reciben mensajes. No se crean carpetas RabbitMq ni Inbox todavía.

## Contrato para el futuro broker

La serialización es JSON con nombres PascalCase, igual que IAM V1. Los tres tipos usan `InvitationChangedIntegrationEvent` con `Kind` igual a `created`, `accepted` o `rejected`. El tipo estable se guarda en `EventType` del outbox y deberá enviarse como metadato/tipo o routing key por el adaptador. La propiedad calculada `EventType` no se duplica en el payload.

Campos: `EventId`, `OperationId`, `InvitationId`, `PatientId`, `PatientName`, `TargetUserId`, `RequesterUserId`, `RelationshipTypeId`, `RelationshipName`, `Status`, `ExpiresAt`, `Kind`, `OccurredAtUtc`, `Version` (1).

Cada comando exitoso obtiene un OperationId y EventId; los reintentos de publicación conservan ambos. La correlación entre creación y resolución usa InvitationId. OperationId no tiene restricción única: admite varias publicaciones de una operación futura.

No se incluyen contraseñas, JWT, teléfonos ni datos clínicos. Nombre y relación del paciente son datos de Care incluidos para construir el aviso. NotificationCommunication deberá obtener el nombre del solicitante de IAM y construir el título y texto equivalentes al monolito. El worker no depende de un contexto HTTP ni consulta IAM.

## Atomicidad

La creación requiere obtener el ID de la invitación desde la base. `ExecuteInTransactionAsync` abre una transacción local: el primer guardado obtiene el ID; después se agrega el evento y se hace el segundo guardado; solo entonces se confirma. Un fallo revierte ambos guardados.

Aceptación/rechazo agregan el outbox antes del único guardado. EF Core persiste el cambio de invitación, la vinculación cuando corresponda y el evento en la misma transacción local.

El outbox no tiene FK hacia la invitación: una eliminación posterior no borra la evidencia pendiente de publicación.

## Activación y publicación

`IntegrationMessaging:Enabled` es `false` por defecto, `PollIntervalSeconds` es 5 y `BatchSize` es 20. Intervalo positivo y lotes entre 1 y 100 se validan al iniciar.

Sin adaptador, mantener deshabilitado. Habilitar sin registrar IIntegrationEventPublisher hace fallar el inicio explícitamente. Deshabilitado, el worker no accede a la base ni marca eventos como enviados.

El futuro adaptador deberá completar PublishAsync solo después de aceptación durable del transporte; respetar cancelación y timeout finito; preservar identificadores; y lanzar excepción ante fallo o resultado incierto. Para RabbitMQ se requieren publisher confirms, mensajes persistentes, topología durable y detección de publicaciones sin ruta.

El worker usa PostgreSQL `FOR UPDATE SKIP LOCKED` para reclamar lotes entre réplicas. Registra fallos con espera exponencial hasta 300 segundos; solo marca PublishedAtUtc tras confirmación. Mantiene la transacción del lote durante la publicación, igual que IAM V1: usar lotes pequeños y timeout acotado en el adaptador.

La entrega es al menos una vez. Si el broker acepta y falla el commit local, se reenviará el mismo EventId. Los futuros consumidores necesitan deduplicación y deben tolerar reordenamientos; este worker no garantiza orden por invitación entre fallos y réplicas. No hay límite de intentos ni cola de errores en esta etapa; los mensajes problemáticos quedan pendientes con su último error.

La migración se aplica al arrancar si ApplyMigrationsOnStartup está habilitado. No se aplicó a una base PostgreSQL real durante esta implementación.

## Verificación

Ejecutar desde la raíz del microservicio:

```powershell
dotnet run --project tests/Care.V1.Checks/Care.V1.Checks.csproj
```

Las comprobaciones usan repositorios, servicio, UnitOfWork y outbox reales con SQLite en memoria para probar commit y rollback. Verifican reglas, destinatarios, contratos, eventos pendientes, reintentos, configuración de EF, generación del SQL de migración PostgreSQL y activación del worker. No se conectan a PostgreSQL ni al broker; aún faltan pruebas del transporte real y de concurrencia PostgreSQL cuando se integre.