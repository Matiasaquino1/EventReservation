import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { EventService } from '../../core/services/event.service';
import { EventModel } from '../../core/models/event.model';

@Component({
  selector: 'app-event-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <h1>Eventos disponibles</h1>

    <p *ngIf="!loading && events.length === 0">
      No hay eventos disponibles
    </p>

    <div class="grid" *ngIf="!loading && events.length > 0">
      <div class="card" *ngFor="let event of events">
        <h3>{{ event.title }}</h3>
        <p>{{ event.location }}</p>
        <p>{{ event.eventDate | date:'shortDate' }}</p>
        <p class="price">$ {{ event.price }}</p>

        <a [routerLink]="['/events', event.eventId]">
          Ver detalle
        </a>
      </div>
    </div>
  `,
  styles: [`
    .grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
      gap: 1.5rem;
    }

    .card {
      padding: 1rem;
      border-radius: 8px;
      border: 1px solid #ddd;
      transition: all .2s ease;
    }

    .card:hover {
      transform: translateY(-4px);
      box-shadow: 0 8px 20px rgba(0,0,0,.15);
    }

    .price {
      font-weight: bold;
    }
  `]
})
export class EventListComponent implements OnInit {

  events: EventModel[] = [];
  loading = true;

  constructor(private eventService: EventService) {}

  ngOnInit(): void {
    this.eventService.getEvents({ page: 1, limit: 10 }).subscribe({
      next: events => {
        this.events = events;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }
}


