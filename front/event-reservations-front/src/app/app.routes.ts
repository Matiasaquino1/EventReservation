import { Routes } from '@angular/router';
import { EventListComponent } from './features/events/event-list.component';
import { EventDetailComponent } from './features/events/event-detail.component';
import { LoginComponent } from './features/auth/login.component';
import { RegisterComponent } from './features/auth/register.component';
import { MyReservationsComponent } from './features/reservations/my-reservations.component';
import { ReservationCreateComponent } from './features/reservations/reservation-create.component';
import { UsersComponent } from './features/admin/users.component';
import { ReservationsAdminComponent } from './features/admin/reservations-admin.component';
import { AuthGuard } from './core/guards/auth.guard';
import { RoleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  { path: '', component: EventListComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'events/:id', component: EventDetailComponent },
  { path: 'my-reservations', component: MyReservationsComponent, canActivate: [AuthGuard] },
  { path: 'reservations/create', component: ReservationCreateComponent, canActivate: [AuthGuard] },
  {
    path: 'admin',
    canActivate: [RoleGuard],
    data: { roles: ['Admin'] },
    children: [
      { path: 'users', component: UsersComponent },
      { path: 'reservations', component: ReservationsAdminComponent }
    ]
  },
  { path: '**', redirectTo: '' }
];