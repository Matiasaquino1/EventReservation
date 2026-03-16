🎟️ Event Reservations API (.NET 8)
API REST robusta desarrollada en ASP.NET Core para la gestión integral de eventos, reservas y procesamiento de pagos seguros. Este backend está diseñado bajo estándares de nivel productivo, priorizando la integridad de los datos y una arquitectura escalable.

🚀 Características Principales
Seguridad de Nivel Empresarial: Autenticación y autorización mediante JWT con control de acceso basado en roles (RBAC).

Arquitectura en Capas: Implementación de patrones Repository y Service para un código limpio, testeable y mantenible.

Manejo de Estados con Enums: Control estricto de flujos de reserva y pagos, evitando errores por strings "mágicos".

Stock Management: Control automático y atómico de disponibilidad de entradas durante el ciclo de vida de la reserva.

Filtros Avanzados: Endpoints administrativos con soporte para búsqueda, paginación y filtrado dinámico.

💳 Flujo de Pago Webhook-Safe (Stripe)
A diferencia de implementaciones básicas, este sistema garantiza la consistencia de los datos mediante el procesamiento asíncrono. La lógica crítica de negocio (descuento de stock y confirmación) solo ocurre tras la validación de la firma de Stripe.

El proceso:
Reserva: El usuario crea una reserva (Pending).

Intención: Se genera un PaymentIntent en Stripe vía POST /api/payments.

Notificación: Stripe notifica el éxito/fallo mediante un Webhook.

Sincronización: El sistema valida la firma, actualiza el estado del pago y confirma la reserva de forma automática.

🧱 Estructura del Proyecto
El código está organizado siguiendo principios de Separación de Preocupaciones (SoC):
EventReservations
├── Controllers    # Endpoints de la API (Auth, Events, Payments, Webhooks)
├── Services       # Lógica de negocio e integraciones (IEventService, IPaymentService)
├── Repositories   # Acceso a datos mediante Entity Framework Core
├── Models         # Entidades de dominio y Enums (ReservationStatuses, PaymentStatuses)
├── Dto            # Objetos de transferencia de datos para entrada/salida
├── Data           # Contexto de Base de Datos (PostgreSQL)
└── Profiles       # Configuraciones de AutoMapper

🔐 Configuración y Seguridad
El sistema requiere las siguientes claves en el appsettings.json para funcionar correctamente:
{
  "Stripe": {
    "SecretKey": "sk_test_...",
    "WebhookSecret": "whsec_..."
  },
  "Jwt": {
    "Key": "tu_clave_secreta_super_larga",
    "Issuer": "EventReservations",
    "Audience": "EventReservationsUsers"
  }
}

▶️ Ejecución en Local
Restaurar dependencias:

Bash:
dotnet restore
Aplicar migraciones (PostgreSQL):

Bash:
dotnet ef database update

Correr la API:
Bash:
dotnet run

Escuchar Webhooks de Stripe (Opcional):
Bash:
stripe listen --forward-to https://localhost:5001/api/stripe/webhook

🧠 Decisiones de Diseño
Idempotencia: Los pagos y reservas están protegidos contra duplicación de registros.

Webhooks Independientes: El StripeWebhooksController está desacoplado del flujo normal para permitir validaciones de firma específicas de Stripe.

Mapping: Uso extensivo de AutoMapper para proteger las entidades de dominio y no exponerlas directamente en la API.

Validación de Ownership: Los usuarios solo pueden acceder a sus propias reservas mediante validación del userId extraído del Token JWT.

🧑‍💻 Autor
Matías Aquino - Backend Developer especializado en .NET / ASP.NET Core
[LinkedIn](https://www.linkedin.com/in/matias-aquino-988752187/) | Portfolio
