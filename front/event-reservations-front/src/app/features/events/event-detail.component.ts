import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';

import { EventService } from '../../core/services/event.service';
import { AuthService } from '../../core/services/auth.service';
import { Event } from '../../core/models/event.model';

@Component({
  selector: 'app-event-detail',
  standalone: true,
  imports: [CommonModule, MatButtonModule, RouterLink],
  template: `
    <div *ngIf="event">
      <h2>{{ event.title }}</h2>
      <p>{{ event.description }}</p>

      <p>
        Fecha:
        {{ event.eventDate
          ? (event.eventDate | date:'shortDate')
          : '-' }}
      </p>

      <p>Ubicación: {{ event.location }}</p>
      <p>Precio: {{ event.price }}</p>
      <p>Disponibles: {{ event.ticketsAvailable }}</p>

      <button
        mat-raised-button
        color="primary"
        *ngIf="authService.currentUser"
        [routerLink]="['/reservations/create']"
        [queryParams]="{ eventId: event.eventId }">
        Reservar
      </button>

      <button
        mat-raised-button
        *ngIf="authService.hasRole('Organizer') || authService.hasRole('Admin')"
        [routerLink]="['/admin/events', event.eventId]">
        Editar
      </button>
    </div>
  `
})
export class EventDetailComponent implements OnInit {

  event: Event | null = null;

  constructor(
    private route: ActivatedRoute,
    private eventService: EventService,
    public authService: AuthService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) return;

    this.eventService.getEvent(id)
      .subscribe(e => this.event = e);
  }
}
