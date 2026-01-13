import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router'
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ReservationService } from '../../core/services/reservation.service';
import { PaymentService } from '../../core/services/payment.service';
import { EventService } from '../../core/services/event.service';
import { Event } from '../../core/models/event.model';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { loadStripe } from '@stripe/stripe-js';
import { environment } from '../../../enviroments/environment';

@Component({
  selector: 'app-reservation-create',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, MatButtonModule, MatInputModule],
  template: `
    <div *ngIf="event">
      <h2>Reservar: {{ event.title }}</h2>
      <form [formGroup]="form" (ngSubmit)="onSubmit()">
        <mat-form-field>
          <input matInput formControlName="numberOfTickets" type="number" placeholder="Número de Tickets">
        </mat-form-field>
        <button mat-raised-button color="primary" type="submit">Reservar y Pagar</button>
      </form>
    </div>
  `
})
export class ReservationCreateComponent implements OnInit {
  form: FormGroup;
  event: Event | null = null;
  stripe: any;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private reservationService: ReservationService,
    private paymentService: PaymentService,
    private eventService: EventService
  ) {
    this.form = this.fb.group({ numberOfTickets: [1, Validators.required] });
    this.stripe = loadStripe(environment.stripePublishableKey);
  }

  ngOnInit() {
    const eventId = +this.route.snapshot.queryParams['eventId'];
    this.eventService.getEvent(eventId).subscribe(e => this.event = e);
  }

  async onSubmit() {
    if (!this.event) return;
    const reservation = await this.reservationService.createReservation({
      eventId: this.event.eventId,
      numberOfTickets: this.form.value.numberOfTickets
    }).toPromise();
    const paymentIntent = await this.paymentService.createPaymentIntent(this.event.price * this.form.value.numberOfTickets).toPromise();
    // Aquí integra Stripe Elements para procesar el pago (ejemplo simplificado)
    // Redirige a confirmación después
    this.router.navigate(['/my-reservations']);
  }
}