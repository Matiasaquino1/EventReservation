import { Component, inject, computed } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { switchMap } from 'rxjs/operators';
import { of } from 'rxjs';
import { toObservable } from '@angular/core/rxjs-interop';

import { EventService } from '../../core/services/event.service';
import { EventFormComponent } from '../events/event-form.component';
import { EventModel } from '../../core/models/event.model';

@Component({
  selector: 'app-event-admin',
  standalone: true,
  imports: [EventFormComponent],
  template: `
    <div class="admin-container">
      <h1>{{ isEdit() ? 'Editar Evento' : 'Crear Nuevo Evento' }}</h1>
      
      <app-event-form 
        [initialData]="event()" 
        (save)="handleSave($event)" />
    </div>
  `
})
export class EventAdminComponent {
  private eventService = inject(EventService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  private idParam = toSignal(this.route.paramMap);
  eventId = computed(() => {
    const id = this.idParam()?.get('id');
    return id ? Number(id) : null;
  });

  isEdit = computed(() => !!this.eventId());

  event = toSignal<EventModel | null>(
    toObservable(this.eventId).pipe(
      switchMap(id => id ? this.eventService.getEvent(id) : of(null))
    ),
    { initialValue: null }
  );

  handleSave(formData: Partial<EventModel>) {
    const payload: Partial<EventModel> = {
      ...formData
    };

    if (formData.eventDate) {
      payload.eventDate = new Date(formData.eventDate).toISOString();
    }

    const request$ = this.isEdit()
      ? this.eventService.updateEvent(this.eventId()!, payload)
      : this.eventService.createEvent(payload);

    request$.subscribe(() => this.router.navigate(['/']));
  }
}