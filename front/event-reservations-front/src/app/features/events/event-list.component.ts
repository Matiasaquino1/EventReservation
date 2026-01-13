import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { EventService } from '../../core/services/event.service';
import { Event } from '../../core/models/event.model';

@Component({
  selector: 'app-event-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div *ngIf="loading">Loading...</div>

    <div>
      <input [(ngModel)]="filters.location" placeholder="Ubicación">
      <input [(ngModel)]="filters.date" type="date">
      <input [(ngModel)]="filters.maxPrice" type="number" placeholder="Precio máximo">
      <button (click)="loadEvents()">Filtrar</button>
    </div>

    <div *ngFor="let event of events">
      <div class="card">
        <h3>{{ event.title }}</h3>
        <p>{{ event.description }}</p>
        <p>Fecha: {{ event.eventDate | date:'shortDate' }}</p>
        <p>Precio: {{ event.price }}</p>
        <p>Disponibles: {{ event.ticketsAvailable }}</p>
        <a [routerLink]="['/events', event.eventId]">Ver Detalle</a>
      </div>
    </div>

    <button (click)="prevPage()" [disabled]="page === 1">Anterior</button>
    <button (click)="nextPage()" [disabled]="page * limit >= total">Siguiente</button>
  `,
  styles: [
    '.card { border: 1px solid #ccc; padding: 1rem; margin: 1rem; }'
  ]
})
export class EventListComponent implements OnInit {

  events: Event[] = [];
  loading = false;

  filters = {
    location: '',
    date: '',
   maxPrice: undefined as number | undefined
  };

  page = 1;
  limit = 10;
  total = 0;

  constructor(private eventService: EventService) {}

  ngOnInit(): void {
    this.loadEvents();
  }

  loadEvents(): void {
  this.loading = true;

  this.eventService.getEvents({
    location: this.filters.location,
    date: this.filters.date,
    maxPrice: this.filters.maxPrice,
    page: this.page,
    limit: this.limit
  }).subscribe({
    next: (response) => {
      this.events = response.events;   
      this.total = response.total;
      this.loading = false;
    },
    error: () => {
      this.loading = false;
    }
  });
}

  nextPage(): void {
    if (this.page * this.limit < this.total) {
      this.page++;
      this.loadEvents();
    }
  }

  prevPage(): void {
    if (this.page > 1) {
      this.page--;
      this.loadEvents();
    }
  }
}
