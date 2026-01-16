import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';

import { EventService } from '../../core/services/event.service';
import { EventModel } from '../../core/models/event.model';

@Component({
  selector: 'app-event-admin',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    CommonModule,
    MatButtonModule,
    MatInputModule
  ],
  template: `
    <form [formGroup]="form" (ngSubmit)="onSubmit()">

      <mat-form-field>
        <input matInput formControlName="title" placeholder="Título">
      </mat-form-field>

      <mat-form-field>
        <textarea matInput formControlName="description" placeholder="Descripción"></textarea>
      </mat-form-field>

      <mat-form-field>
        <input matInput type="datetime-local" formControlName="eventDate">
      </mat-form-field>

      <mat-form-field>
        <input matInput formControlName="location" placeholder="Ubicación">
      </mat-form-field>

      <mat-form-field>
        <input matInput type="number" formControlName="price" placeholder="Precio">
      </mat-form-field>

      <mat-form-field>
        <input matInput type="number" formControlName="totalTickets" placeholder="Total Tickets">
      </mat-form-field>

      <button mat-raised-button color="primary" type="submit">
        Guardar
      </button>
    </form>
  `
})
export class EventAdminComponent implements OnInit {

  form: FormGroup;
  eventId: number | null = null;

  constructor(
    private fb: FormBuilder,
    private eventService: EventService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.form = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      eventDate: [''],
      location: ['', Validators.required],
      price: [0, Validators.required],
      totalTickets: [0, Validators.required]
    });
  }

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) return;

    this.eventId = id;

    this.eventService.getEvent(id).subscribe(event => {
      this.form.patchValue({
        ...event,
        eventDate: event.eventDate
          ? new Date(event.eventDate).toISOString().slice(0, 16)
          : ''
      });
    });
  }

  onSubmit(): void {
    const formValue = this.form.value;

    const payload: Partial<EventModel> = {
      ...formValue,
      eventDate: formValue.eventDate
        ? new Date(formValue.eventDate).toISOString()
        : null
    };

    const request$ = this.eventId
      ? this.eventService.updateEvent(this.eventId, payload)
      : this.eventService.createEvent(payload);

    request$.subscribe(() => this.router.navigate(['/']));
  }
}


