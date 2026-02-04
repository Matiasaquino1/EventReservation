import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ActivatedRoute, Router } from '@angular/router';

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
  notFound = false;

  filters: EventFilters = {
    location: '',
    date: '',
    availability: undefined
  };

  constructor(
    private eventService: EventService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.filters = {
        location: params['location'] || '',
        date: params['date'] || '',
        availability: params['availability']
          ? Number(params['availability'])
          : undefined
      };

      this.loadEvents();
    });
  }


  loadEvents(): void {
    this.loading = true;
    this.notFound = false;

    this.eventService.getEvents(this.filters).subscribe({
      next: events => {
        this.events = events;
        this.notFound = events.length === 0;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.notFound = true;
      }
    });
  }

  onSearch(): void {
    this.router.navigate([], {
      queryParams: {
        location: this.filters.location || null,
        date: this.filters.date || null,
        availability: this.filters.availability || null
      },
      queryParamsHandling: 'merge'
    });
  }


  onReset(): void {
    this.router.navigate([], {
      queryParams: {}
    });
  }
}

