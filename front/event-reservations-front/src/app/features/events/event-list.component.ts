import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { EventService } from '../../core/services/event.service';
import { EventModel } from '../../core/models/event.model';
import { EventFilters } from '../../core/models/event-filters.model';

@Component({
  selector: 'app-event-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './event-list.component.html',
  styleUrls: ['./event-list.component.css']
})
export class EventListComponent implements OnInit {

  events: EventModel[] = [];
  loading = false;

  filters: EventFilters = {
    location: '',
    date: '',
    minTickets: undefined,
    maxPrice: undefined,
    page: 1,
    limit: 10
  };

  constructor(private eventService: EventService) {}

  ngOnInit(): void {
    this.loadEvents();
  }

  loadEvents(): void {
    this.loading = true;

    this.eventService.getEvents(this.filters).subscribe({
      next: events => {
        this.events = events;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  onSearch(): void {
    this.filters.page = 1;
    this.loadEvents();
  }

  onReset(): void {
    this.filters = {
      location: '',
      date: '',
      minTickets: undefined,
      maxPrice: undefined,
      page: 1,
      limit: 10
    };
    this.loadEvents();
  }
}


