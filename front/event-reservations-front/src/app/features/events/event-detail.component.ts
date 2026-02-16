import { Component, OnInit, DestroyRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
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

  private destroyRef = inject(DestroyRef);

  event: EventModel | null = null;
  loading = true;
  notFound = false;

  constructor(
    private route: ActivatedRoute,
    private eventService: EventService
  ) {}

  ngOnInit(): void {
    this.route.paramMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(params => {
        const id = Number(params.get('id'));

        if (!id || Number.isNaN(id)) {
          this.notFound = true;
          this.loading = false;
          return;
        }

        this.loading = true;

        this.eventService.getEvent(id)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: event => {
              this.event = event;
              this.notFound = false;
              this.loading = false;
            },
            error: () => {
              this.notFound = true;
              this.loading = false;
            }
          });
      });
  }
}

