import { Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { RoleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () =>
      import('./features/events/event-list/event-list.component')
        .then(m => m.EventListComponent)
  },
  { path: 'login', loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent) },
  { path: 'register', loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent) },
  
  // Rutas de Usuario
  { path: 'events/:id', loadComponent: () => import('./features/events/event-detail/event-detail.component').then(m => m.EventDetailComponent), canActivate: [AuthGuard] },
  { path: 'my-reservations', loadComponent: () => import('./features/reservations/my-reservations/my-reservations.component').then(m => m.MyReservationsComponent), canActivate: [AuthGuard] },
  { path: 'reservations/create', loadComponent: () => import('./features/reservations/reservation-create/reservation-create.component').then(m => m.ReservationCreateComponent), canActivate: [AuthGuard]},
  { path: 'reservations/create/:eventId', loadComponent: () => import('./features/reservations/reservation-create/reservation-create.component').then(m => m.ReservationCreateComponent), canActivate: [AuthGuard]},
  { path: 'payments', loadComponent: () => import('./features/payments/components/payment.component').then(m => m.PaymentComponent), canActivate: [AuthGuard]},
  { path: 'success', loadComponent:  () => import('./features/payments/success/success.component').then(m => m.SuccessComponent), canActivate: [AuthGuard]},

  // Panel de Administración 
  {
    path: 'admin',
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Admin'] },
    children: [
      { path: '', redirectTo: 'management', pathMatch: 'full' },
      { 
        path: 'management', 
        loadComponent: () => import('./features/admin/event-management/event-management.component').then(m => m.EventManagementComponent) 
      },
      { 
        path: 'events/new', 
        loadComponent: () => import('./features/admin/event-form/event-form.component').then(m => m.EventFormComponent) 
      },
      { 
        path: 'events/edit/:id', 
        loadComponent: () => import('./features/admin/event-form/event-form.component').then(m => m.EventFormComponent) 
      },

      { 
        path: 'events/:id/attendees', 
        loadComponent: () => import('./features/admin/event-attendees/event-attendees.component').then(m => m.EventAttendeesComponent) 
      },
      {
        path: 'dashboard-stats',
        loadComponent: () => import('./features/admin/dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent)
      },
      // Otros componentes de admin
      {
        path: 'users', 
        loadComponent: () => import('./features/admin/user-management/users.component').then(m => m.UsersComponent)
      },
      { path: 'reservations', loadComponent: () => import('./features/admin/components/reservations-admin.component').then(m => m.ReservationsAdminComponent) }
    ]
  },
  
  { path: '**', redirectTo: '' } // Catch-all para rutas inexistentes
];