import { Routes } from '@angular/router';
import { UsersComponent } from './features/admin/users.component';
import { ReservationsAdminComponent } from './features/admin/reservations-admin.component';
import { AuthGuard } from './core/guards/auth.guard';
import { RoleGuard } from './core/guards/role.guard';

export const routes: Routes = [

  { path: 'login', loadComponent: () => import('./features/auth/login.component').then(m => m.LoginComponent) },

  { path: 'register', loadComponent: () => import('./features/auth/register.component').then(m => m.RegisterComponent) },

  { path: 'events/:id', loadComponent: () => import('./features/events/event-detail.component').then(m => m.EventDetailComponent) },

  { path: 'my-reservations', loadComponent: () => import('./features/reservations/my-reservations.component').then(m => m.MyReservationsComponent) },

  { path: 'reservations/create/:eventId', loadComponent: () => import('./features/reservations/reservation-create.component').then(m => m.ReservationCreateComponent) },

  {
    path: 'admin',
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Admin'] },
    children: [
      { path: 'users', component: UsersComponent },
      { path: 'reservations', component: ReservationsAdminComponent }
    ]
  },

  // 👇 Home al final
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () =>
      import('./features/events/event-list.component')
        .then(m => m.EventListComponent)
  },

  { path: '**', redirectTo: '' }
];
