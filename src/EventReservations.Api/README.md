## 🎟️ Event Reservations API

API REST desarrollada en ASP.NET Core (.NET) para la gestión de eventos, reservas y pagos online mediante Stripe.
El sistema está diseñado con una arquitectura en capas, manejo de estados con enums y un flujo de pagos 100% webhook-safe.

## 🚀 Características principales

Autenticación y autorización con JWT

Gestión de usuarios y roles (User / Admin)

CRUD de eventos con stock de entradas

Sistema de reservas con control de disponibilidad

Integración completa con Stripe Payment Intents

Procesamiento de pagos asíncrono y seguro vía webhooks

Estados tipados con Enums (no strings)

Arquitectura limpia (Controllers / Services / Repositories)

Soporte para paginación y filtros en endpoints administrativos

## 🧱 Arquitectura del proyecto
EventReservations
|   appsettings.json
|   Program.cs
|   
+---Controllers
|       AdminController.cs
|       AuthController.cs
|       ErrorController.cs
|       EventsController.cs
|       PaymentsController.cs
|       ReservationsController.cs
|       StripeWebhooksController.cs
|       UsersController.cs
|       
+---Data
|       ApplicationDbContext.cs
|       
+---Dto
|       AdjustStockDto.cs
|       AdminReservationDto.cs
|       CreatedReservationDto.cs
|       CreateEventDto.cs
|       CreatePaymentIntentDto.cs
|       ...
|
|       
+---Models
|       Event.cs
|       Payment.cs
|       PaymentStatuses.cs
|       Reservation.cs
|       ReservationStatuses.cs
|       User.cs
|       
+---Profiles
|       MappingProfile.cs
|       
+---Properties
|       launchSettings.json
|       
+---Repositories
|       EventRepository.cs
|       IEventRepository.cs
|       IPaymentRepository.cs
|       IReservationRepository.cs
|       IUserRepository.cs
|       PaymentRepository.cs
|       ReservationRepository.cs
|       UserRepository.cs
|       
+---Services
|       IAuthService.cs
|       IEventService.cs
|       IJwtService.cs
|       IPaymentService.cs
|       IReservationService.cs
|       IUserService.cs
|
+---Program.cs
\---appsettings.json
## 🔁 Flujo de pago con Stripe (Webhook-safe)

El sistema NO confirma pagos desde el frontend.
Toda la lógica crítica ocurre desde el webhook de Stripe, garantizando consistencia.

## 📌 Flujo completo

Usuario crea una reserva
Estado: ReservationStatuses.Pending

Frontend solicita crear el PaymentIntent

POST /api/payments/create-payment-intent


Stripe procesa el pago

Stripe notifica vía webhook

POST /api/stripe/webhook


El webhook:

Actualiza Payment.Status

Confirma la reserva

Descuenta entradas

Maneja idempotencia

## 🧾 Estados del dominio
ReservationStatuses
Pending
Confirmed
Cancelled

PaymentStatuses
Pending
Succeeded
Failed
Canceled


✔ Tipados
✔ Evita errores por strings
✔ Fácil mantenimiento

## 🔐 Seguridad

Autenticación JWT

Autorización por roles

Webhooks sin JWT, validados por firma de Stripe

Validación de ownership (userId) en reservas y pagos

## ⚙️ Configuración
appsettings.json
"Stripe": {
  "SecretKey": "sk_test_xxx",
  "WebhookSecret": "whsec_xxx"
},
"Jwt": {
  "Key": "super_secret_key",
  "Issuer": "EventReservations",
  "Audience": "EventReservationsUsers"
}

## ▶️ Ejecutar el proyecto
dotnet restore
dotnet ef database update
dotnet run


API disponible en:

https://localhost:5001

## 🧪 Testing de Webhooks (local)
stripe listen --forward-to https://localhost:5001/api/stripe/webhook

## 🧠 Decisiones de diseño

Stripe Webhook separado del PaymentsController

Estados gestionados por Enums

Servicios sin lógica de infraestructura

Repositorios sin lógica de negocio

Idempotencia en pagos y reservas

Frontend desacoplado del resultado final del pago

## 🎯 Objetivo del proyecto

Proyecto desarrollado con foco en:

Buenas prácticas backend

Preparación para entorno productivo

Presentación profesional para entrevistas laborales

## 🧑‍💻 Autor

Matías Aquino /
Backend Developer – .NET / ASP.NET Core
