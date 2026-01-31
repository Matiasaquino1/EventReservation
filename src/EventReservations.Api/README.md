{Event Reservations API}

API REST desarrollada en ASP.NET Core (.NET) para la gestión de eventos, reservas y pagos online mediante Stripe.
El sistema está diseñado con una arquitectura en capas, manejo de estados con enums y un flujo de pagos 100% webhook-safe.

*Características principales;

-Autenticación y autorización con JWT
-Gestión de usuarios y roles (User / Admin)
-CRUD de eventos con stock de entradas
-Sistema de reservas con control de disponibilidad
-Integración completa con Stripe Payment Intents
-Procesamiento de pagos asíncrono y seguro vía webhooks
-Estados tipados con Enums (no strings)
-Arquitectura limpia (Controllers / Services / Repositories)
-Soporte para paginación y filtros en endpoints administrativos

🧱 Arquitectura del proyecto
EventReservations
│
├── Controllers
│   ├── AuthController.cs
│   ├── UsersController.cs
│   ├── EventsController.cs
│   ├── ReservationsController.cs
│   ├── PaymentsController.cs
│   ├── StripeWebhooksController.cs
│   └── AdminController.cs
│
├── Services
│   ├── IAuthService.cs
│   ├── IUserService.cs
│   ├── IEventService.cs
│   ├── IReservationService.cs
│   ├── IPaymentService.cs
│   └── IJwtService.cs
│
├── Repositories
│   ├── IUserRepository.cs
│   ├── IEventRepository.cs
│   ├── IReservationRepository.cs
│   ├── IPaymentRepository.cs
│   ├── UserRepository.cs
│   ├── EventRepository.cs
│   ├── ReservationRepository.cs
│   └── PaymentRepository.cs
│
├── Models
│   ├── User.cs
│   ├── Event.cs
│   ├── Reservation.cs
│   ├── Payment.cs
│   ├── ReservationStatuses.cs
│   └── PaymentStatuses.cs
│
├── DTOs
│   ├── CreateEventDto.cs
│   ├── CreatePaymentIntentDto.cs
│   ├── ReservationDto.cs
│   ├── PaymentDto.cs
│   └── ...
│
├── Data
│   └── ApplicationDbContext.cs
│
├── Profiles
│   └── MappingProfile.cs
│
├── Program.cs
└── appsettings.json

🔁 Flujo de pago con Stripe (Webhook-safe)

El sistema NO confirma pagos desde el frontend.
Toda la lógica crítica ocurre desde el webhook de Stripe, garantizando consistencia.

📌 Flujo completo

1.Usuario crea una reserva
Estado: ReservationStatuses.Pending

2.Frontend solicita crear el PaymentIntent:
POST /api/payments/create-payment-intent

3.Stripe procesa el pago

4.Stripe notifica vía webhook
POST /api/stripe/webhook

5.El webhook:
-Actualiza Payment.Status
-Confirma la reserva
-Descuenta entradas
-Maneja idempotencia

🧾 Estados del dominio
*ReservationStatuses:
Pending
Confirmed
Cancelled

*PaymentStatuses_
Pending
Succeeded
Failed
Canceled

✔ Tipados
✔ Evita errores por strings
✔ Fácil mantenimiento

🔐 Seguridad

-Autenticación JWT
-Autorización por roles
-Webhooks sin JWT, validados por firma de Stripe
-Validación de ownership (userId) en reservas y pagos

⚙️ Configuración
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

▶️ Ejecutar el proyecto:
dotnet restore
dotnet ef database update
dotnet run


API disponible en:
https://localhost:5001

Testing de Webhooks (local):
stripe listen --forward-to https://localhost:5001/api/stripe/webhook

🧠 Decisiones de diseño:

-Stripe Webhook separado del PaymentsController
-Estados gestionados por Enums
-Servicios sin lógica de infraestructura
-Repositorios sin lógica de negocio
-Idempotencia en pagos y reservas
-Frontend desacoplado del resultado final del pago

🎯 Objetivo del proyecto

Proyecto desarrollado con foco en:

Buenas prácticas backend

Preparación para entorno productivo

Presentación profesional para entrevistas laborales

🧑‍💻 Autor:

Matías Aquino
Backend Developer – .NET / ASP.NET Core
