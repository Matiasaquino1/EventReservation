import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { Router } from '@angular/router';

import { ReservationService } from '../../core/services/reservation.service';
import { Reservation } from '../../core/models/reservation.model';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog.component';

@Component({
  selector: 'app-my-reservations',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatDialogModule],
  templateUrl: './my-reservations.component.html',
  styleUrls: ['./my-reservations.component.css']
})
export class MyReservationsComponent implements OnInit {

  reservations = signal<Reservation[]>([]);
  loading = true;

  private readonly PAYMENT_KEY = 'pending_payment_reservation_id';

  constructor(
    private reservationService: ReservationService,
    private dialog: MatDialog,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadReservations();
  }

  loadReservations(): void {
    this.reservationService.getMyReservations()
      .subscribe({
        next: res => {
          this.reservations.set(res);
          this.loading = false;
        },
        error: () => {
          this.loading = false;
        }
      });
  }

  goToPay(reservationId: number): void {
    // Guarda el pago quedó en progreso
    localStorage.setItem(this.PAYMENT_KEY, reservationId.toString());

    this.router.navigate(['/payment', reservationId]);
  }

  isPaymentInProgress(reservationId: number): boolean {
    return localStorage.getItem(this.PAYMENT_KEY) === reservationId.toString();
  }

  cancel(reservationId: number): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Cancelar Reserva',
        message: '¿Estás seguro?'
      }
    });

    dialogRef.afterClosed().subscribe(confirm => {
      if (!confirm) return;

      this.reservationService.cancelReservation(reservationId)
        .subscribe(() => {
          this.reservations.update(list =>
            list.filter(r => r.reservationId !== reservationId)
          );

          // Si cancela la que estaba en pago, limpia el estado
          if (this.isPaymentInProgress(reservationId)) {
            localStorage.removeItem(this.PAYMENT_KEY);
          }
        });
    });
  }
}
