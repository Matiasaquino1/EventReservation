import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { switchMap } from 'rxjs/operators';
import { of } from 'rxjs';
import { EventService } from '../../core/services/event.service';
import { EventModel } from '../../core/models/event.model';

@Component({
  selector: 'app-event-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './event-detail.component.html',
  styleUrls: ['./event-detail.component.css']
})
export class EventDetailComponent implements OnInit {

  event: EventModel | null = null;
  loading = true;
  notFound = false;

  constructor(
    private route: ActivatedRoute,
    private eventService: EventService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.pipe(
      switchMap(params => {
        const id = Number(params.get('id') ?? params.get('eventId'));

        if (!id || Number.isNaN(id)) {
          this.notFound = true;
          this.loading = false;
          return of(null);
        }

        this.loading = true;
        this.notFound = false;
        return this.eventService.getEvent(id);
      })
    ).subscribe({
      next: event => {
        this.event = event;
        this.loading = false;
      },
      error: () => {
        this.notFound = true;
        this.loading = false;
      }
    });
  }
}
