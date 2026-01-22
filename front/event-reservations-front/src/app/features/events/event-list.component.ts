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

  filters: EventFilters = {
    location: '',
    date: '',
    minTickets: undefined,
    maxPrice: undefined,
    page: 1,
    limit: 10
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
      minTickets: params['minTickets']
        ? Number(params['minTickets'])
        : undefined,
      maxPrice: params['maxPrice']
        ? Number(params['maxPrice'])
        : undefined,
      page: params['page']
        ? Number(params['page'])
        : 1,
      limit: this.filters.limit ?? 10
    };

    this.loadEvents();
  });
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

  this.router.navigate([], {
    queryParams: {
      location: this.filters.location || null,
      date: this.filters.date || null,
      minTickets: this.filters.minTickets || null,
      maxPrice: this.filters.maxPrice || null,
      page: this.filters.page
    },
    queryParamsHandling: 'merge'
  });
  }


  onReset(): void {
  this.router.navigate([], {
    queryParams: {}
  });
  }

  nextPage(): void {
  this.router.navigate([], {
    queryParams: {
      page: (this.filters.page ?? 1) + 1
    },
    queryParamsHandling: 'merge'
  });
  }

  prevPage(): void {
  if ((this.filters.page ?? 1) <= 1) return;

  this.router.navigate([], {
    queryParams: {
      page: (this.filters.page ?? 1) - 1
    },
    queryParamsHandling: 'merge'
  });
  }


}


