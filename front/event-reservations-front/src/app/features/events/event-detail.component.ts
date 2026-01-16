import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { EventService } from '../../core/services/event.service';
import { EventModel } from '../../core/models/event.model';
import { EventFilters } from '../../core/models/event-filters.model';

@Component({
  selector: 'app-event-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, MatCardModule, MatButtonModule, MatProgressSpinnerModule],
  template: `
    <div *ngIf="loading">Cargando evento...</div>

    <div *ngIf="!loading && event">
      <h1>{{ event.title }}</h1>

      <p>{{ event.description }}</p>

      <p>
        Fecha:
        {{ event.eventDate | date:'shortDate' }}
      </p>

      <p>Ubicación: {{ event.location }}</p>
      <p>Precio: {{ event.price | currency:'ARS' }}</p>
      <p>Disponibles: {{ event.ticketsAvailable }}</p>

      <a routerLink="/">← Volver a eventos</a>
    </div>

    <p *ngIf="!loading && !event">
      Evento no encontrado
    </p>
  `
})
export class EventDetailComponent implements OnInit {

  event: EventModel | null = null;
  loading = true;

  constructor(
    private route: ActivatedRoute,
    private eventService: EventService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) return;

    this.eventService.getEvent(id).subscribe({
      next: event => {
        this.event = event;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }
}
