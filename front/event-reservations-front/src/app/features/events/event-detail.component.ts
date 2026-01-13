import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { EventService } from '../../core/services/event.service';
import { AuthService } from '../../core/services/auth.service';
import { Event } from '../../core/models/event.model';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-event-detail',
  standalone: true,
  imports: [CommonModule, MatButtonModule],
  template: `
    <div *ngIf="event">
      <h2>{{ event.title }}</h2>
      <p>{{ event.description }}</p>
      <p>Fecha: {{ event.eventDate | date }}</p>
      <p>Ubicación: {{ event.location }}</p>
      <p>Precio: {{ event.price }}</p>
      <p>Disponibles: {{ event.ticketsAvailable }}</p>
      <button mat-raised-button color="primary" [routerLink]="['/reservations/create', { eventId: event.eventId }]" *ngIf="authService.currentUser">Reservar</button>
      <button mat-raised-button *ngIf="authService.hasRole('Organizer') || authService.hasRole('Admin')" [routerLink]="['/admin/events', event.eventId]">Editar</button>
    </div>
  `
})
export class EventDetailComponent implements OnInit {
  event: Event | null = null;

  constructor(private route: ActivatedRoute, private eventService: EventService, public authService: AuthService, private router: Router) {}

  ngOnInit() {
    const id = +this.route.snapshot.paramMap.get('id')!;
    this.eventService.getEvent(id).subscribe(e => this.event = e);
  }
}