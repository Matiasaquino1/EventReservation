import { Routes } from '@angular/router';
import { EventListComponent } from './features/events/event-list/event-list.component';
import { EventDetailComponent } from './features/events/event-detail/event-detail.component';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { MyReservationsComponent } from './features/reservations/my-reservations/my-reservations.component';
import { ReservationCreateComponent } from './features/reservations/reservation-create/reservation-create.component';
import { UsersComponent } from './features/admin/components/users.component';
import { ReservationsAdminComponent } from './features/admin/components/reservations-admin.component';
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
  { path: 'events/:id', loadComponent: () => import('./features/events/event-detail/event-detail.component').then(m => m.EventDetailComponent), canActivate: [AuthGuard] },
  { path: 'my-reservations', loadComponent: () => import('./features/reservations/my-reservations/my-reservations.component').then(m => m.MyReservationsComponent), canActivate: [AuthGuard] },
  { path: 'reservations/create', loadComponent: () => import('./features/reservations/reservation-create/reservation-create.component').then(m => m.ReservationCreateComponent), canActivate: [AuthGuard]},
  { path: 'reservations/create/:eventId', loadComponent: () => import('./features/reservations/reservation-create/reservation-create.component').then(m => m.ReservationCreateComponent), canActivate: [AuthGuard]},
  { path: 'payments', loadComponent: () => import('./features/payments/components/payment.component').then(m => m.PaymentComponent), canActivate: [AuthGuard]},
  { path: 'success', loadComponent:  () => import('./features/payments/success/success.component').then(m => m.SuccessComponent), canActivate: [AuthGuard]},
  {
    path: 'admin',
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Admin'] },
    children: [
      { path: 'users', loadComponent: () => import('./features/admin/components/users.component').then(m => m.UsersComponent), canActivate: [AuthGuard] },
      { path: 'reservations', loadComponent: () => import('./features/admin/components/reservations-admin.component').then(m => m.ReservationsAdminComponent), canActivate: [AuthGuard] }
    ]
  },
  { path: '', redirectTo: '/home', pathMatch: 'full' },
];
