# Consulta de destinatarios

`GET /internal/patients/{patientId}/notification-recipients` ejecuta `GetPatientNotificationRecipientsQuery` en `PatientQueryService`, siguiendo la estructura de consultas internas de Care.

Migra las reglas de `PatientNotificationAcl` del monolito: cuidador principal, cuidadores asociados, cuidador actual únicamente si pertenece a los anteriores, usuarios distintos y contactos de emergencia. Incluye nombres/teléfonos obtenidos mediante el puerto `IUserInfoGateway`. Un usuario ausente en IAM conserva el nombre `Usuario {id}`.

La consulta es utilizada por el worker de Notifications sin una sesión de cuidador. Se protege con `X-Foll-Service-Key`, comparado con `InternalApi:BackgroundQueryKey`. Care reenvía esa credencial solo hacia el endpoint mínimo de IAM `/internal/iam/users/{id}/notification-recipient`. El gateway conserva JWT y ruta pública de IAM para las operaciones de usuario; no guarda un JWT del usuario en eventos ni solicita tokens de servicio.

Las peticiones que ya existen de `device-access` y `emergency-access` conservan JWT. No se amplió el acceso de la llave a las API de usuario.

La llave de desarrollo está directamente en `appsettings.json` y coincide con el valor configurado para estas consultas en Notifications/IAM. No requiere migración de base de datos.

Verificación: `dotnet run --project tests/Care.V1.Checks/Care.V1.Checks.csproj`. Las pruebas verifican los destinatarios con SQLite, el fallback de usuario y la separación entre JWT y consulta de contexto; con `--rabbitmq` también verifican el transporte real.
