import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';

import { ReservationService } from '../../core/services/reservation.service';
import { Reservation } from '../../core/models/reservation.model';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog.component';

@Component({
  selector: 'app-my-reservations',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatDialogModule],
  template: `
    <h2>Mis Reservas</h2>

    <div *ngFor="let res of reservations" class="card">
      <p>Evento: {{ res.event?.title ?? '-' }}</p>
      <p>Estado: {{ res.status }}</p>
      <p>Tickets: {{ res.numberOfTickets }}</p>

      <button
        mat-raised-button
        color="warn"
        (click)="cancel(res.reservationId)">
        Cancelar
      </button>
    </div>
  `,
  styles: [`
    .card {
      border: 1px solid #ccc;
      padding: 1rem;
      margin: 1rem 0;
    }
  `]
})
export class MyReservationsComponent implements OnInit {

  reservations: Reservation[] = [];

  constructor(
    private reservationService: ReservationService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadReservations();
  }

  loadReservations(): void {
    this.reservationService.getMyReservations()
      .subscribe(res => this.reservations = res);
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
          this.reservations = this.reservations
            .filter(r => r.reservationId !== reservationId);
        });
    });
  }
}
