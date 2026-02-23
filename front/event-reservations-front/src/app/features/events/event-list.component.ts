import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ActivatedRoute, Router } from '@angular/router';
import { BehaviorSubject, Subject, switchMap, takeUntil } from 'rxjs';

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
  totalCount = 0;

  filters: EventFilters = {
    location: '',
    date: '',
    availability: undefined
  };

  private readonly filters$ = new BehaviorSubject<EventFilters>({
    page: 1,
    pageSize: 10
  });
  private readonly destroy$ = new Subject<void>();

  constructor(
    private eventService: EventService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.filters$
      .pipe(
        takeUntil(this.destroy$),
        switchMap(filters => {
          this.loading = true;
          this.notFound = false;
          return this.eventService.getEvents(filters);
        })
      )
      .subscribe({
        next: events => {
          this.events = events;
          this.totalCount = events.length;
          this.notFound = events.length === 0;
          this.loading = false;
        },
        error: () => {
          this.events = [];
          this.totalCount = 0;
          this.loading = false;
          this.notFound = true;
        }
      });

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

        this.filters$.next({
          ...this.filters,
          page: 1,
          pageSize: 10
        });
      });

    this.onSearch();
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

    this.filters$.next({
      ...this.filters,
      page: 1,
      pageSize: 10
    });
  }

  onReset(): void {
    this.filters = {
      location: '',
      date: '',
      availability: undefined
    };

    this.router.navigate([], {
      queryParams: {}
    });

    this.filters$.next({
      page: 1,
      pageSize: 10
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
