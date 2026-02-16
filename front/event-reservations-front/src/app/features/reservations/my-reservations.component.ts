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
    <section class="reservations">
      <h2>Mis Reservas</h2>

      <p class="loading" *ngIf="loading">Cargando reservas...</p>

      <div *ngIf="!loading && reservations.length === 0" class="empty">
        Aún no tienes reservas. Explora eventos y reserva tu lugar.
      </div>

      <div *ngFor="let res of reservations" class="card">
        <div>
          <p class="title">{{ res.event?.title ?? '-' }}</p>
          <p class="meta">Estado: {{ res.status }}</p>
          <p class="meta">Tickets: {{ res.numberOfTickets }}</p>
        </div>

        <button
          mat-raised-button
          color="warn"
          (click)="cancel(res.reservationId)">
          Cancelar
        </button>
      </div>
    </section>
  `,
  styles: [`
    .reservations {
      max-width: 720px;
      margin: 0 auto;
    }
    .card {
      border: 1px solid #e2e8f0;
      padding: 1rem 1.5rem;
      margin: 1rem 0;
      border-radius: 14px;
      background: #ffffff;
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 1rem;
      box-shadow: 0 12px 24px rgba(15, 23, 42, 0.08);
    }
    .title {
      font-weight: 700;
      margin-bottom: 0.25rem;
    }
    .meta {
      color: #64748b;
      margin: 0.15rem 0;
    }
    .loading {
      color: #5f6b7a;
      padding: 1.5rem 0;
    }
    .empty {
      padding: 1.5rem;
      background: #f8fafc;
      border-radius: 12px;
      color: #64748b;
    }
  `]
})
export class MyReservationsComponent implements OnInit {

  reservations: Reservation[] = [];
  loading = true;

  constructor(
    private reservationService: ReservationService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadReservations();
  }

  loadReservations(): void {
    this.reservationService.getMyReservations()
      .subscribe({
        next: res => {
          this.reservations = res;
          this.loading = false;
        },
        error: () => {
          this.loading = false;
        }
      });
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
