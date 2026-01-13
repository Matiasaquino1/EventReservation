import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';

import { firstValueFrom } from 'rxjs';
import { loadStripe, Stripe } from '@stripe/stripe-js';

import { ReservationService } from '../../core/services/reservation.service';
import { PaymentService } from '../../core/services/payment.service';
import { EventService } from '../../core/services/event.service';
import { Event } from '../../core/models/event.model';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-reservation-create',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatInputModule
  ],
  template: `
    <div *ngIf="event; else loading">
      <h2>Reservar: {{ event.title }}</h2>

      <form [formGroup]="form" (ngSubmit)="onSubmit()">
        <mat-form-field appearance="outline">
          <mat-label>Número de tickets</mat-label>
          <input
            matInput
            type="number"
            formControlName="numberOfTickets"
            min="1"
            [max]="event.ticketsAvailable"
          />
        </mat-form-field>

        <button
          mat-raised-button
          color="primary"
          type="submit"
          [disabled]="form.invalid || submitting"
        >
          Reservar y pagar
        </button>
      </form>
    </div>

    <ng-template #loading>
      <p>Cargando evento...</p>
    </ng-template>
  `
})
export class ReservationCreateComponent implements OnInit {

  form!: FormGroup;
  event!: Event;
  stripe!: Stripe | null;
  submitting = false;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private reservationService: ReservationService,
    private paymentService: PaymentService,
    private eventService: EventService
  ) {}

  async ngOnInit(): Promise<void> {
    this.form = this.fb.group({
      numberOfTickets: [1, [Validators.required, Validators.min(1)]]
    });

    this.stripe = await loadStripe(environment.stripePublishableKey);

    const eventId = Number(this.route.snapshot.paramMap.get('eventId'));
    if (!eventId) {
      this.router.navigate(['/']);
      return;
    }

    this.event = await firstValueFrom(
      this.eventService.getEvent(eventId)
    );
  }

  async onSubmit(): Promise<void> {
    if (!this.event || this.form.invalid) return;

    this.submitting = true;

    try {
      const reservation = await firstValueFrom(
        this.reservationService.createReservation({
          eventId: this.event.eventId,
          numberOfTickets: this.form.value.numberOfTickets
        })
      );

      await firstValueFrom(
        this.paymentService.createPaymentIntent(
          this.event.price * this.form.value.numberOfTickets
        )
      );

      // 👉 Acá después podés integrar Stripe Elements
      this.router.navigate(['/my-reservations']);

    } catch (error) {
      console.error('Error al crear reserva o pago', error);
    } finally {
      this.submitting = false;
    }
  }
}
