import { Component, inject, computed, signal } from '@angular/core'; // Añadimos signal
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { toSignal, toObservable } from '@angular/core/rxjs-interop';
import { switchMap, map, tap } from 'rxjs/operators';
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
export class EventListComponent {
  private eventService = inject(EventService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  filters = signal<EventFilters>({
    location: this.route.snapshot.queryParams['location'] || '',
    date: this.route.snapshot.queryParams['date'] || '',
    availability: this.route.snapshot.queryParams['availability'] ? Number(this.route.snapshot.queryParams['availability']) : undefined
  });

  private queryParams$ = this.route.queryParams;
  
  isLoading = signal(false);

  eventsResource = toSignal(
    this.queryParams$.pipe(
      tap(() => this.isLoading.set(true)),
      switchMap(params => {
        const filtersFromUrl: EventFilters = {
          location: params['location'] || '',
          date: params['date'] || '',
          availability: params['availability'] ? Number(params['availability']) : undefined
        };
        this.filters.set(filtersFromUrl);
        return this.eventService.getEvents(filtersFromUrl);
      }),
      tap(() => this.isLoading.set(false))
    ),
    { initialValue: [] as EventModel[] }
  );

  onSearch(): void {
    const currentFilters = this.filters();
    this.router.navigate([], {
      queryParams: {
        location: currentFilters.location || null,
        date: currentFilters.date || null,
        availability: currentFilters.availability ?? null
      }
    });
  }

  onReset(): void {
    this.filters.set({ location: '', date: '', availability: undefined });
    this.router.navigate([], { queryParams: {} });
  }
}