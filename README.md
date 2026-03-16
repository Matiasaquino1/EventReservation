EventReservations - Sistema de Gestión de Reservas
EventReservations es una plataforma integral para la publicación, búsqueda y reserva de eventos en tiempo real. El proyecto aplica una arquitectura moderna desacoplada, utilizando .NET para el procesamiento robusto de datos y Angular para una experiencia de usuario fluida y reactiva.

🚀 Tecnologías Utilizadas
Backend (.NET 8)
Entity Framework Core: Gestión de base de datos con PostgreSQL.

AutoMapper: Mapeo eficiente entre Entidades y DTOs.

ASP.NET Core Identity: Seguridad y autenticación basada en roles (Admin, Organizer, User).

Arquitectura: Repository Pattern y servicios desacoplados.

Frontend (Angular 17+)
Signals: Gestión de estado reactiva y eficiente.

Standalone Components: Arquitectura moderna sin módulos pesados.

RxJS: Manejo de flujos de datos asíncronos y filtros dinámicos.

CSS Moderno: Diseño responsivo con Flexbox y una estética de "píldora" unificada.

Base de Datos
PostgreSQL: Almacenamiento relacional con manejo de zonas horarias (UTC).

✨ Características Principales
Buscador Inteligente: Filtros combinados por título, ubicación y fecha.

Sistema de Roles: * Usuarios: Pueden ver eventos y gestionar sus propias reservas.

Administradores: Panel de control total para crear, editar, pausar o cancelar eventos.

Gestión de Inventario: Control automático de disponibilidad de tickets (TicketsAvailable) al crear o modificar eventos.

Diseño UX: Navbar persistente con acceso rápido a funciones según el rol del usuario.

👤 Autor
Matias Aquino - [LinkedIn](https://www.linkedin.com/in/matias-aquino-988752187/) - Portfolio
