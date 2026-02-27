import { Component, inject, computed, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { toSignal, toObservable } from '@angular/core/rxjs-interop';
import { switchMap, of, map } from 'rxjs';

import { ReservationService } from '../../core/services/reservation.service';
import { EventService } from '../../core/services/event.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-reservation-create',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reservation-create.component.html',
  styleUrls: ['./reservation-create.component.css']
})
export class ReservationCreateComponent {
  private route = inject(ActivatedRoute);
  private eventService = inject(EventService);
  private reservationService = inject(ReservationService);
  public router = inject(Router);
  public authService = inject(AuthService);

  // 1. Capturamos el ID desde Params o QueryParams usando Signals
  private params = toSignal(this.route.paramMap);
  private queryParams = toSignal(this.route.queryParamMap);

  eventId = computed(() => {
    const id = this.params()?.get('eventId') || this.queryParams()?.get('eventId');
    return id ? Number(id) : null;
  });

  // 2. Cargamos el evento reactivamente
  event = toSignal(
    toObservable(this.eventId).pipe(
      switchMap(id => {
        if (!id) return of(null);
        return this.eventService.getEvent(id);
      })
    ),
    { initialValue: undefined } 
  );

  // 3. Estados de la reserva
  numberOfTickets = signal(1);
  isProcessing = signal(false);
  error = signal('');
  loading = computed(() => this.event() === undefined);

  // 4. Lógica Pro: Cálculo de precio total automático
  totalPrice = computed(() => {
    const ev = this.event();
    return ev ? ev.price * this.numberOfTickets() : 0;
  });

  goToPayment() {
    const currentEvent = this.event();
    if (!currentEvent) return;

    this.isProcessing.set(true);

    // crear la reserva y obtener el ID generado por .NET
    this.reservationService.createReservation({
      eventId: currentEvent.eventId,
      numberOfTickets: this.numberOfTickets()
    }).subscribe({
      next: (res: any) => {
        // 2. 'res.reservationId' es el ID de la bd que .NET nos devuelve al crear la reserva
        this.router.navigate(['/payments/'], { 
          queryParams: { reservationId: res.reservationId } 
        });
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Error al crear la reserva');
        this.isProcessing.set(false);
      }
    });
  }


  reserve(): void {
    const currentEvent = this.event();
    if (!currentEvent || this.numberOfTickets() < 1) return;

    if (!this.authService.currentUser) {
      this.error.set('Debes iniciar sesión para reservar.');
      return;
    }

    this.isProcessing.set(true); // Para deshabilitar el botón y mostrar loading

    this.reservationService.createReservation({
      eventId: currentEvent.eventId,
      numberOfTickets: this.numberOfTickets()
    }).subscribe({
      next: (res: any) => {
        // ✅ Ahora navegamos a pagos con el ID que nos dio el Back
        this.router.navigate(['/payments'], { 
          queryParams: { reservationId: res.reservationId } 
        });
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Error al reservar');
        this.isProcessing.set(false);
      }
    });   
  }
}
