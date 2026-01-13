import { Component, OnInit } from '@angular/core';
import { ReservationService } from '../../core/services/reservation.service';
import { Reservation } from '../../core/models/reservation.model';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatDialogModule } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog.component';

@Component({
  selector: 'app-my-reservations',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatDialogModule],
  template: `
    <h2>Mis Reservas</h2>
    <div *ngFor="let res of reservations" class="card">
      <p>Evento: {{ res.event?.title }}</p>
      <p>Estado: {{ res.status }}</p>
      <p>Tickets: {{ res.numberOfTickets }}</p>
      <button mat-raised-button color="warn" (click)="cancel(res.reservationId)">Cancelar</button>
    </div>
  `,
  styles: ['.card { border: 1px solid #ccc; padding: 1rem; margin: 1rem; }']
})
export class MyReservationsComponent implements OnInit {
  reservations: Reservation[] = [];

  constructor(private reservationService: ReservationService, private dialog: MatDialog) {}

  ngOnInit() {
    this.reservationService.getMyReservations().subscribe(r => this.reservations = r);
  }

  cancel(id: number) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Cancelar Reserva', message: '¿Estás seguro?' }
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result) this.reservationService.cancelReservation(id).subscribe(() => {
        this.reservations = this.reservations.filter(r => r.reservationId !== id);
      });
    });
  }
}