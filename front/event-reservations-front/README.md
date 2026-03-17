# EventReservations - Frontend (Angular)

Este módulo maneja toda la interfaz de usuario y la lógica de consumo de APIs.

## 🛠️ Características Técnicas
* **State Management:** Implementación de **Angular Signals** para un manejo de estado granular y eficiente.
* **Componentes Standalone:** Estructura moderna que elimina la necesidad de NgModules.
* **Reactive Forms:** Validaciones complejas para la creación de eventos y perfiles de usuario.
* **Pasarela de Pago:** Integración de **Stripe Elements** para capturar pagos de forma segura sin que los datos sensibles toquen nuestro servidor.

## 📂 Estructura de Carpetas
* `src/app/core`: Servicios globales, interceptores y guards de autenticación.
* `src/app/shared`: Componentes reutilizables (Navbar con diseño de píldora, botones, etc.).
* `src/app/features`: Módulos de funcionalidad (Eventos, Admin, Reservas).

## 🚀 Ejecución
1. `npm install`
2. `ng serve`
