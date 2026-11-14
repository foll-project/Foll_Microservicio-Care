# Consulta síncrona de estado del dispositivo

Care usa `IDeviceStatusGateway` y `DeviceStatusGateway`, registrados mediante `AddHttpClient`, para completar el campo `device` al crear o consultar pacientes y al listar pacientes por cuidador. Reenvía el encabezado Authorization recibido, siguiendo el patrón actual de IAM.

## Contrato

`GET {Services:DeviceManagment:BaseUrl}/internal/devices/patient/{patientId}/status`

- 200: JSON compatible con `PatientDeviceStatusInfo`; conserva los campos del estado del dispositivo del monolito.
- 404: el paciente autorizado no tiene dispositivo; Care devuelve `device: { isLinked: false }`.
- 401, 403, fallos de red y otros errores HTTP: la consulta falla. No se convierten en ausencia de dispositivo.

La respuesta enriquecida de Care conserva el formato del monolito. Los cálculos de conectividad y batería baja pertenecen a DeviceManagment.

## Configuración local actual

`Services:DeviceManagment:BaseUrl` apunta a `http://localhost:5237/`, el perfil HTTP de foll-backend. Puede cambiarse mediante `Services__DeviceManagment__BaseUrl`.

No existe un proyecto independiente de DeviceManagment en este workspace. El endpoint receptor está implementado en `foll-backend/DeviceManagment/Interfaces/Internal/InternalDevicesController.cs`, como implementación ejecutable de referencia para su extracción posterior. El monolito requiere el SDK de .NET 10.

El receptor valida el JWT y permite leer el estado al cuidador principal y a los cuidadores vinculados. La vinculación y desvinculación del dispositivo mantienen sus permisos previos. La comprobación de lectura usa los datos de acceso de Care del monolito: para probar esta conexión temporal, ambos lados deben disponer de los mismos identificadores y relaciones de pacientes y cuidadores, y usar una configuración JWT compatible. No se sincronizan bases de datos mediante esta integración.

Al separar DeviceManagment, deben migrarse el endpoint y su autorización, sustituyendo la consulta local al ACL de Care por la integración correspondiente. Las rutas internas deben mantenerse fuera del acceso público de Kong.

## Verificación realizada

Care compila en .NET 8 sin errores ni advertencias. Se ejecutaron 16 comprobaciones aisladas con los archivos reales del gateway, endpoint y servicio de permisos: principal, cuidador invitado, usuario ajeno, JWT ausente, paciente inexistente, dispositivo ausente, contrato JSON, reenvío del JWT y propagación de errores. No se utilizaron bases de datos ni servicios reales. No se compiló el monolito completo porque el entorno solo tiene instalado el SDK de .NET 8.