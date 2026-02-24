import { Component, input, output, inject, effect } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { CommonModule } from '@angular/common';
import { EventModel } from '../../core/models/event.model';

@Component({
  selector: 'app-event-form',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, MatButtonModule, MatInputModule],
  templateUrl: './event-form.component.html',
  styleUrls: ['./event-form.component.css']
})
export class EventFormComponent {
  private fb = inject(FormBuilder);

  initialData = input<EventModel | null>(null);

  save = output<Partial<EventModel>>();

  form = this.fb.group({
    title: ['', Validators.required],
    description: [''],
    eventDate: [''],
    location: ['', Validators.required],
    price: [0, [Validators.required, Validators.min(0)]],
    totalTickets: [0, [Validators.required, Validators.min(1)]]
  });

  constructor() {
    effect(() => {
      const event = this.initialData();
      if (event) {
        this.form.patchValue({
          ...event,
          eventDate: event.eventDate 
            ? new Date(event.eventDate).toISOString().slice(0, 16) 
            : ''
        });
      }
    });
  }

  onSubmit() {
    if (this.form.valid) {
      this.save.emit(this.form.value as Partial<EventModel>);
    }
  }
}