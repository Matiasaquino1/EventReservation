import { Component, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { rxResource, toSignal } from '@angular/core/rxjs-interop';
import { of } from 'rxjs';
import { EventService } from '../../core/services/event.service';
import { switchMap } from 'rxjs/operators';
import { toObservable } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-event-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './event-detail.component.html',
  styleUrls: ['./event-detail.component.css']
})
export class EventDetailComponent {
  private route = inject(ActivatedRoute);
  private eventService = inject(EventService);

  // 1. Convertimos paramMap a Signal
  private params = toSignal(this.route.paramMap);

  // 2. Extraemos el ID
  private eventId = computed(() => {
    const p = this.params();
    return p ? Number(p.get('id')) : null;
  });

  // Alternativa ultra-estable con toSignal y switchMap
   // Necesitas import { toObservable }
  private eventId$ = toObservable(this.eventId);

  eventSignal = toSignal(
    this.eventId$.pipe(
      switchMap(id => id ? this.eventService.getEvent(id) : of(null))
    )
  );
}
