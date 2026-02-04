import { Component } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, CommonModule],
  template: `
    <nav class="nav">
      <a routerLink="/" class="logo">Event Reservations</a>

      <div class="links">
        <a routerLink="/">Eventos</a>
        <a routerLink="/my-reservations" *ngIf="authService.currentUser">Mis Reservas</a>
        <a routerLink="/admin/users" *ngIf="authService.hasRole('Admin')">Usuarios</a>
        <a routerLink="/admin/reservations" *ngIf="authService.hasRole('Admin')">Reservas</a>
      </div>

      <div class="actions">
        <span class="user" *ngIf="authService.currentUser as user">
          Hola, {{ user.username }}
        </span>
        <a routerLink="/register" *ngIf="!authService.currentUser">Crear cuenta</a>
        <a routerLink="/login" *ngIf="!authService.currentUser">Ingresar</a>
        <button class="logout" (click)="onLogout()" *ngIf="authService.currentUser">Salir</button>
      </div>
    </nav>
  `,
  styles: [`
    .nav {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 1.5rem;
      padding: 1rem 2rem;
      background: #ffffff;
      box-shadow: 0 10px 30px rgba(15, 23, 42, 0.08);
      position: sticky;
      top: 0;
      z-index: 10;
    }
    .logo {
      font-weight: 700;
      color: #1976d2;
      text-decoration: none;
    }
    .links, .actions {
      display: flex;
      align-items: center;
      gap: 1rem;
    }
    a {
      text-decoration: none;
      color: #1f2a37;
      font-weight: 500;
    }
    .actions a {
      color: #1976d2;
      font-weight: 600;
    }
    .user {
      color: #475569;
      font-weight: 600;
    }
    .logout {
      border: none;
      background: #1976d2;
      color: #ffffff;
      padding: 0.5rem 1rem;
      border-radius: 999px;
      cursor: pointer;
      font-weight: 600;
    }
  `]
})
export class NavbarComponent {
  constructor(public authService: AuthService, private router: Router) {}

  onLogout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
