import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { EventService } from '../../core/services/event.service';
import { Event } from '../../core/models/event.model';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-event-admin',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, MatButtonModule, MatInputModule],
  template: `
    <form [formGroup]="form" (ngSubmit)="onSubmit()">
      <mat-form-field>
        <input matInput formControlName="title" placeholder="Título">
      </mat-form-field>
      <mat-form-field>
        <textarea matInput formControlName="description" placeholder="Descripción"></textarea>
      </mat-form-field>
      <mat-form-field>
        <input matInput formControlName="eventDate" type="datetime-local">
      </mat-form-field>
      <mat-form-field>
        <input matInput formControlName="location" placeholder="Ubicación">
      </mat-form-field>
      <mat-form-field>
        <input matInput formControlName="price" type="number" placeholder="Precio">
      </mat-form-field>
      <mat-form-field>
        <input matInput formControlName="totalTickets" type="number" placeholder="Total Tickets">
      </mat-form-field>
      <button mat-raised-button color="primary" type="submit">Guardar</button>
    </form>
  `
})
export class EventAdminComponent implements OnInit {
  form: FormGroup;
  eventId: number | null = null;

  constructor(private fb: FormBuilder, private eventService: EventService, private route: ActivatedRoute, private router: Router) {
    this.form = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      eventDate: [''],
      location: ['', Validators.required],
      price: [0, Validators.required],
      totalTickets: [0, Validators.required]
    });
  }

  ngOnInit() {
    this.eventId = +this.route.snapshot.paramMap.get('id')! || null;
    if (this.eventId) {
      this.eventService.getEvent(this.eventId).subscribe(e => this.form.patchValue(e));
    }
  }

  onSubmit() {
    const event = this.form.value;
    if (this.eventId) {
      this.eventService.updateEvent(this.eventId, event).subscribe(() => this.router.navigate(['/']));
    } else {
      this.eventService.createEvent(event).subscribe(() => this.router.navigate(['/']));
    }
  }
}