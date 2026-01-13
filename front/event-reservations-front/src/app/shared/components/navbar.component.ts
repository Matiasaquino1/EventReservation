import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, CommonModule],
  template: `
    <nav>
      <a routerLink="/">Eventos</a>
      <a routerLink="/my-reservations" *ngIf="authService.currentUser">Mis Reservas</a>
      <a routerLink="/admin/users" *ngIf="authService.hasRole('Admin')">Admin Usuarios</a>
      <a routerLink="/admin/reservations" *ngIf="authService.hasRole('Admin')">Admin Reservas</a>
      <button (click)="authService.logout()" *ngIf="authService.currentUser">Logout</button>
      <a routerLink="/login" *ngIf="!authService.currentUser">Login</a>
    </nav>
  `,
  styles: ['nav { display: flex; gap: 1rem; padding: 1rem; background: #1976d2; color: white; }']
})
export class NavbarComponent {
  constructor(public authService: AuthService) {}
}