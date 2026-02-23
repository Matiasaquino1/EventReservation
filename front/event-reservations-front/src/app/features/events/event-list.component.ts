import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';

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
export class EventListComponent implements OnInit, OnDestroy {

  events: EventModel[] = [];
  loading = false;
  notFound = false;

  filters: EventFilters = {
    location: '',
    date: '',
    availability: undefined
  };

  private readonly destroy$ = new Subject<void>();
  cdr: any;

  constructor(
    private eventService: EventService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.route.queryParams
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {

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

    this.eventService.getEvents(this.filters)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: events => {
          this.events = events;
          this.notFound = events.length === 0;
          this.loading = false;
          this.cdr.detectChanges();
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
        availability: this.filters.availability ?? null
      }
    });
  }

  onReset(): void {
    this.router.navigate([], {
      queryParams: {}
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
