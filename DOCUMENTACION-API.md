# Foll-Care API Documentation

## 1. POST `/api/care/patients`

### Header

| Campo | Descripcion |
| --- | --- |
| Authorization | Bearer {jwt} |

### Request

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| dni | String | DNI del paciente |
| firstName | String | Nombre del paciente |
| lastName | String | Apellido del paciente |
| birthDate | DateOnly | Fecha de nacimiento |
| relationshipTypeId | Short | Tipo de relacion del guardián oficial |
| bloodType | BloodType | Grupo sanguineo |
| medicalConditions | Dictionary<String, String>? | Condiciones medicas |
| medications | Dictionary<String, String>? | Medicamentos |

### Request Example

```json
{
  "dni": "3231014-1",
  "firstName": "Daniel",
  "lastName": "Clark",
  "birthDate": "1992-04-18",
  "relationshipTypeId": 1,
  "bloodType": 7,
  "medicalConditions": {
    "hypertension": "controlled"
  },
  "medications": {
    "amlodipine": "5mg daily"
  }
}
```

### Success 200

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| patientId | Long | Identificador creado cuando la respuesta no incluye el agregado completo |
| PatientId | Long | Identificador del paciente |
| Dni | String | DNI |
| FirstName | String | Nombre |
| LastName | String | Apellido |
| BirthDate | DateOnly | Fecha de nacimiento |
| BloodType | BloodType | Grupo sanguineo |
| MedicalConditions | Object | Condiciones medicas |
| Medications | Object | Medicamentos |
| CurrentGuardianUserId | Long? | Guardián actual |
| OfficialGuardianUserId | Long | Guardián oficial |
| caregivers | Array | Cuidadores vinculados |
| EmergencyContacts | Array | Contactos de emergencia |
| Invitations | Array | Invitaciones del paciente |

### Success Response Example

```json
HTTP/1.1 200 OK
{
  "PatientId": 15,
  "Dni": "3231014-1",
  "FirstName": "Daniel",
  "LastName": "Clark",
  "BirthDate": "1992-04-18",
  "BloodType": 7,
  "MedicalConditions": {
    "hypertension": "controlled"
  },
  "Medications": {
    "amlodipine": "5mg daily"
  },
  "CurrentGuardianUserId": 3,
  "OfficialGuardianUserId": 3,
  "caregivers": [],
  "EmergencyContacts": [],
  "Invitations": []
}
```

### Error 4xx

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| message | String | Mensaje devuelto por la excepcion de negocio |

### Error Response Example

```json
HTTP/1.1 400 BAD_REQUEST
{
  "message": "Error de validacion o negocio"
}
```

## 2. GET `/api/care/patients/{id}`

### Header

| Campo | Descripcion |
| --- | --- |
| Authorization | Bearer {jwt} |

### Request

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| id | Long | Identificador del paciente |

### Request Example

```http
GET /api/care/patients/15
Authorization: Bearer eyJhbGciOi...
```

### Success 200

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| PatientId | Long | Identificador del paciente |
| Dni | String | DNI |
| FirstName | String | Nombre |
| LastName | String | Apellido |
| BirthDate | DateOnly | Fecha de nacimiento |
| BloodType | BloodType | Grupo sanguineo |
| MedicalConditions | Object | Condiciones medicas |
| Medications | Object | Medicamentos |
| CurrentGuardianUserId | Long? | Guardián actual |
| OfficialGuardianUserId | Long | Guardián oficial |
| caregivers | Array | Cuidadores vinculados |
| EmergencyContacts | Array | Contactos de emergencia |
| Invitations | Array | Invitaciones del paciente |

### Success Response Example

```json
HTTP/1.1 200 OK
{
  "PatientId": 15,
  "Dni": "3231014-1",
  "FirstName": "Daniel",
  "LastName": "Clark",
  "BirthDate": "1992-04-18",
  "BloodType": 7,
  "MedicalConditions": {
    "hypertension": "controlled"
  },
  "Medications": {
    "amlodipine": "5mg daily"
  },
  "CurrentGuardianUserId": 3,
  "OfficialGuardianUserId": 3,
  "caregivers": [],
  "EmergencyContacts": [],
  "Invitations": []
}
```

### Error 4xx

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| message | String | Paciente no encontrado o mensaje de validacion |

### Error Response Example

```json
HTTP/1.1 404 NOT_FOUND
{
  "message": "Paciente no encontrado."
}
```

## 3. PUT `/api/care/patients/{id}`

### Header

| Campo | Descripcion |
| --- | --- |
| Authorization | Bearer {jwt} |

### Request

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| id | Long | Identificador del paciente |
| firstName | String | Nombre actualizado |
| lastName | String | Apellido actualizado |
| birthDate | DateOnly | Fecha de nacimiento actualizada |
| bloodType | BloodType | Grupo sanguineo |
| medicalConditions | Dictionary<String, String>? | Condiciones medicas |
| medications | Dictionary<String, String>? | Medicamentos |

### Request Example

```json
{
  "firstName": "Daniel",
  "lastName": "Clark",
  "birthDate": "1992-04-18",
  "bloodType": 7,
  "medicalConditions": {
    "hypertension": "controlled"
  },
  "medications": {
    "amlodipine": "5mg daily"
  }
}
```

### Success 200

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| message | String | Paciente actualizado |

### Success Response Example

```json
HTTP/1.1 200 OK
{
  "message": "Paciente actualizado."
}
```

### Error 4xx

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| message | String | Mensaje devuelto por la excepcion de negocio |

### Error Response Example

```json
HTTP/1.1 400 BAD_REQUEST
{
  "message": "Error de validacion o negocio"
}
```

## 4. PUT `/api/care/patients/{id}/guard-shift`

### Header

| Campo | Descripcion |
| --- | --- |
| Authorization | Bearer {jwt} |

### Request

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| id | Long | Identificador del paciente |
| newCurrentGuardianUserId | Long | Nuevo guardián actual |

### Request Example

```json
{
  "newCurrentGuardianUserId": 42
}
```

### Success 200

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| message | String | Turno asignado |

### Success Response Example

```json
HTTP/1.1 200 OK
{
  "message": "Turno asignado."
}
```

### Error 4xx

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| message | String | Mensaje devuelto por la excepcion de negocio |

### Error Response Example

```json
HTTP/1.1 400 BAD_REQUEST
{
  "message": "Error de validacion o negocio"
}
```

## 5. POST `/api/care/patients/{id}/guard-shift/restore`

### Header

| Campo | Descripcion |
| --- | --- |
| Authorization | Bearer {jwt} |

### Request

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| id | Long | Identificador del paciente |

### Request Example

```http
POST /api/care/patients/15/guard-shift/restore
Authorization: Bearer eyJhbGciOi...
```

### Success 200

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| message | String | Turno restaurado |

### Success Response Example

```json
HTTP/1.1 200 OK
{
  "message": "Turno restaurado."
}
```

### Error 4xx

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| message | String | Mensaje devuelto por la excepcion de negocio |

### Error Response Example

```json
HTTP/1.1 400 BAD_REQUEST
{
  "message": "Error de validacion o negocio"
}
```

## 6. POST `/api/care/patients/{id}/emergency-contacts`

### Header

| Campo | Descripcion |
| --- | --- |
| Authorization | Bearer {jwt} |

### Request

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| id | Long | Identificador del paciente |
| fullName | String | Nombre completo del contacto |
| phoneNumber | String | Telefono del contacto |
| relationship | Object/String/Number | Relacion con el paciente |

### Request Example

```json
{
  "fullName": "Maria Lopez",
  "phoneNumber": "+56 9 1234 5678",
  "relationship": "Madre"
}
```

### Success 200

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| emergencyContactId | Long | Identificador del contacto creado |

### Success Response Example

```json
HTTP/1.1 200 OK
{
  "emergencyContactId": 9
}
```

### Error 4xx

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| message | String | Mensaje devuelto por la excepcion de negocio |

### Error Response Example

```json
HTTP/1.1 400 BAD_REQUEST
{
  "message": "Error de validacion o negocio"
}
```

## 7. DELETE `/api/care/patients/{id}/emergency-contacts/{contactId}`

### Header

| Campo | Descripcion |
| --- | --- |
| Authorization | Bearer {jwt} |

### Request

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| id | Long | Identificador del paciente |
| contactId | Long | Identificador del contacto |

### Request Example

```http
DELETE /api/care/patients/15/emergency-contacts/9
Authorization: Bearer eyJhbGciOi...
```

### Success 200

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| message | String | Contacto eliminado |

### Success Response Example

```json
HTTP/1.1 200 OK
{
  "message": "Contacto eliminado."
}
```

### Error 4xx

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| message | String | Mensaje devuelto por la excepcion de negocio |

### Error Response Example

```json
HTTP/1.1 400 BAD_REQUEST
{
  "message": "Error de validacion o negocio"
}
```

## 8. POST `/api/care/patients/{id}/annotations`

### Header

| Campo | Descripcion |
| --- | --- |
| Authorization | Bearer {jwt} |

### Request

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| id | Long | Identificador del paciente |
| content | String | Contenido de la anotacion |

### Request Example

```json
{
  "content": "Tomar presion arterial al despertar."
}
```

### Success 200

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| message | String | Anotacion guardada exitosamente |

### Success Response Example

```json
HTTP/1.1 200 OK
{
  "message": "Anotación guardada exitosamente."
}
```

### Error 4xx

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| message | String | Mensaje devuelto por la excepcion de negocio |

### Error Response Example

```json
HTTP/1.1 400 BAD_REQUEST
{
  "message": "Error de validacion o negocio"
}
```

## 9. GET `/api/care/patients/{id}/annotations`

### Header

| Campo | Descripcion |
| --- | --- |
| Authorization | Bearer {jwt} |

### Request

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| id | Long | Identificador del paciente |

### Request Example

```http
GET /api/care/patients/15/annotations
Authorization: Bearer eyJhbGciOi...
```

### Success 200

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| id | String | Identificador de la anotacion |
| date | String | Fecha ISO 8601 |
| text | String | Texto de la anotacion |
| author | String | Nombre del autor |

### Success Response Example

```json
HTTP/1.1 200 OK
[
  {
    "id": "1",
    "date": "2026-06-07T14:30:00.0000000Z",
    "text": "Tomar presion arterial al despertar.",
    "author": "Daniel Clark"
  }
]
```

### Error 4xx

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| message | String | Mensaje devuelto por la excepcion de negocio |

### Error Response Example

```json
HTTP/1.1 400 BAD_REQUEST
{
  "message": "Error de validacion o negocio"
}
```

## 10. POST `/api/care/patients/{dni}/invitations`

### Header

| Campo | Descripcion |
| --- | --- |
| Authorization | Bearer {jwt} |

### Request

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| dni | String | DNI del paciente |
| relationshipTypeId | Short | Tipo de relacion |

### Request Example

```json
{
  "relationshipTypeId": 1
}
```

### Success 200

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| invitationId | Long | Identificador de la invitacion |

### Success Response Example

```json
HTTP/1.1 200 OK
{
  "invitationId": 33
}
```

### Error 4xx

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| message | String | Mensaje devuelto por la excepcion de negocio |

### Error Response Example

```json
HTTP/1.1 400 BAD_REQUEST
{
  "message": "Error de validacion o negocio"
}
```

## 11. GET `/api/care/patients/{id}/caregivers`

### Header

| Campo | Descripcion |
| --- | --- |
| Authorization | Bearer {jwt} |

### Request

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| id | Long | Identificador del paciente |

### Request Example

```http
GET /api/care/patients/15/caregivers
Authorization: Bearer eyJhbGciOi...
```

### Success 200

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| userId | Long | Identificador del usuario cuidador |
| user | Object | Datos del usuario obtenidos del gateway |
| relationshipTypeId | Short? | Tipo de relacion |
| caregiverKind | String | official o caregiver |

### Success Response Example

```json
HTTP/1.1 200 OK
[
  {
    "userId": 3,
    "user": {
      "firstName": "Daniel",
      "lastName": "Clark"
    },
    "relationshipTypeId": null,
    "caregiverKind": "official"
  }
]
```

### Error 4xx

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| message | String | Solo el OficialGuardian puede ver los cuidadores, o mensaje de negocio |

### Error Response Example

```json
HTTP/1.1 403 FORBIDDEN
{
  "message": "Solo el OficialGuardian puede ver los cuidadores."
}
```

## 12. GET `/api/care/patients/by-caregiver/{caregiverUserId}`

### Header

| Campo | Descripcion |
| --- | --- |
| Authorization | Bearer {jwt} |

### Request

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| caregiverUserId | Long | Identificador del cuidador |

### Request Example

```http
GET /api/care/patients/by-caregiver/3
Authorization: Bearer eyJhbGciOi...
```

### Success 200

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| patient | Object | Paciente completo |
| caregiverKind | String | official o caregiver |
| relationshipTypeId | Short? | Tipo de relacion con el paciente |

### Success Response Example

```json
HTTP/1.1 200 OK
[
  {
    "patient": {
      "PatientId": 15,
      "Dni": "3231014-1"
    },
    "caregiverKind": "official",
    "relationshipTypeId": 1
  }
]
```

### Error 4xx

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| message | String | Solo puedes consultar tus propios pacientes |

### Error Response Example

```json
HTTP/1.1 403 FORBIDDEN
{
  "message": "Solo puedes consultar tus propios pacientes."
}
```

## 13. POST `/api/care/invitations/{invitationId}/accept`

### Header

| Campo | Descripcion |
| --- | --- |
| Authorization | Bearer {jwt} |

### Request

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| invitationId | Long | Identificador de la invitacion |

### Request Example

```http
POST /api/care/invitations/33/accept
Authorization: Bearer eyJhbGciOi...
```

### Success 200

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| message | String | Invitacion aceptada |

### Success Response Example

```json
HTTP/1.1 200 OK
{
  "message": "Invitación aceptada."
}
```

### Error 4xx

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| message | String | Mensaje devuelto por la excepcion de negocio |

### Error Response Example

```json
HTTP/1.1 400 BAD_REQUEST
{
  "message": "Error de validacion o negocio"
}
```

## 14. POST `/api/care/invitations/{invitationId}/reject`

### Header

| Campo | Descripcion |
| --- | --- |
| Authorization | Bearer {jwt} |

### Request

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| invitationId | Long | Identificador de la invitacion |

### Request Example

```http
POST /api/care/invitations/33/reject
Authorization: Bearer eyJhbGciOi...
```

### Success 200

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| message | String | Invitacion rechazada |

### Success Response Example

```json
HTTP/1.1 200 OK
{
  "message": "Invitación rechazada."
}
```

### Error 4xx

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| message | String | Mensaje devuelto por la excepcion de negocio |

### Error Response Example

```json
HTTP/1.1 400 BAD_REQUEST
{
  "message": "Error de validacion o negocio"
}
```

## 15. GET `/api/care/relationship-types`

### Header

| Campo | Descripcion |
| --- | --- |
| Authorization | Bearer {jwt} |

### Request

Sin body.

### Request Example

```http
GET /api/care/relationship-types
Authorization: Bearer eyJhbGciOi...
```

### Success 200

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| RelationshipTypeId | Short | Identificador |
| Name | String | Nombre del tipo de relacion |

### Success Response Example

```json
HTTP/1.1 200 OK
[
  {
    "RelationshipTypeId": 1,
    "Name": "Hijo"
  },
  {
    "RelationshipTypeId": 2,
    "Name": "Madre"
  }
]
```

### Error 4xx

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| message | String | Mensaje devuelto por la excepcion de negocio |

### Error Response Example

```json
HTTP/1.1 400 BAD_REQUEST
{
  "message": "Error de validacion o negocio"
}
```

## 16. GET `/internal/patients/{patientId}/device-access`

### Header

| Campo | Descripcion |
| --- | --- |
| Authorization | Bearer {jwt} |

### Request

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| patientId | Long | Identificador del paciente |

### Request Example

```http
GET /internal/patients/15/device-access
Authorization: Bearer eyJhbGciOi...
```

### Success 200

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| PatientId | Long | Identificador del paciente |
| OfficialGuardianUserId | Long | Guardián oficial |

### Success Response Example

```json
HTTP/1.1 200 OK
{
  "PatientId": 15,
  "OfficialGuardianUserId": 3
}
```

### Error 4xx

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| message | String | Paciente no encontrado o mensaje de validacion |

### Error Response Example

```json
HTTP/1.1 404 NOT_FOUND
{
  "message": "Paciente no encontrado."
}
```

## 17. GET `/internal/patients/{patientId}/emergency-access`

### Header

| Campo | Descripcion |
| --- | --- |
| Authorization | Bearer {jwt} |

### Request

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| patientId | Long | Identificador del paciente |

### Request Example

```http
GET /internal/patients/15/emergency-access
Authorization: Bearer eyJhbGciOi...
```

### Success 200

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| PatientId | Long | Identificador del paciente |
| OfficialGuardianUserId | Long | Guardián oficial |
| CurrentGuardianUserId | Long? | Guardián actual |
| CaregiverUserIds | IReadOnlyCollection<Long> | Cuidadores vinculados |

### Success Response Example

```json
HTTP/1.1 200 OK
{
  "PatientId": 15,
  "OfficialGuardianUserId": 3,
  "CurrentGuardianUserId": 5,
  "CaregiverUserIds": [
    5,
    8,
    12
  ]
}
```

### Error 4xx

| Campo | Tipo | Descripcion |
| --- | --- | --- |
| message | String | Paciente no encontrado o mensaje de validacion |

### Error Response Example

```json
HTTP/1.1 404 NOT_FOUND
{
  "message": "Paciente no encontrado."
}
```

## Coverage

Este documento incluye todos los endpoints encontrados actualmente en el codigo fuente.