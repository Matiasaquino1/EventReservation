import { Component, OnInit } from '@angular/core';
import { ReservationService } from '../../core/services/reservation.service';
import { Reservation } from '../../core/models/reservation.model';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatFormFieldModule } from '@angular/material/form-field';
import { FormsModule } from '@angular/forms';
import { MatSelectModule } from '@angular/material/select';


@Component({
  selector: 'app-reservations-admin',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatButtonModule, MatPaginatorModule, FormsModule, MatSelectModule, MatFormFieldModule],
  template: `
    <h2>Administrar Reservas</h2>
    <div>
      <mat-form-field>
        <mat-label>Estado</mat-label>
        <mat-select [(ngModel)]="filters.status">
          <mat-option value="">Todos</mat-option>
          <mat-option value="Pending">Pending</mat-option>
          <mat-option value="Confirmed">Confirmed</mat-option>
          <mat-option value="Cancelled">Cancelled</mat-option>
        </mat-select>
      </mat-form-field>
      <button mat-raised-button (click)="loadReservations()">Filtrar</button>
    </div>
    <table mat-table [dataSource]="reservations">
      <ng-container matColumnDef="user">
        <th mat-header-cell *matHeaderCellDef>Usuario</th>
        <td mat-cell *matCellDef="let res">{{ res.user?.username }}</td>
      </ng-container>
      <ng-container matColumnDef="event">
        <th mat-header-cell *matHeaderCellDef>Evento</th>
        <td mat-cell *matCellDef="let res">{{ res.event?.title }}</td>
      </ng-container>
      <ng-container matColumnDef="status">
        <th mat-header-cell *matHeaderCellDef>Estado</th>
        <td mat-cell *matCellDef="let res">{{ res.status }}</td>
      </ng-container>
      <ng-container matColumnDef="actions">
        <th mat-header-cell *matHeaderCellDef>Acciones</th>
        <td mat-cell *matCellDef="let res">
          <button mat-button (click)="forceConfirm(res.eventId)">Forzar Confirmación</button>
        </td>
      </ng-container>
      <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
      <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
    </table>
    <mat-paginator [length]="total" [pageSize]="limit" (page)="onPageChange($event)"></mat-paginator>
  `
})
export class ReservationsAdminComponent implements OnInit {
  reservations: Reservation[] = [];
  displayedColumns = ['user', 'event', 'status', 'actions'];
  filters: { status?: string; eventId?: number } = {};
  page = 1;
  limit = 10;
  total = 0;


  constructor(private reservationService: ReservationService) {}

  ngOnInit() {
    this.loadReservations();
  }

  loadReservations() {
    this.reservationService.getReservations(this.filters, this.page, this.limit).subscribe(res => {
      this.reservations = res.reservations;
      this.total = res.total;
    });
  }

  forceConfirm(eventId: number) {
    this.reservationService.forceConfirm(eventId)
    .subscribe(() => this.loadReservations());
  }


  onPageChange(event: PageEvent) {
    this.page = event.pageIndex + 1;
    this.limit = event.pageSize;
    this.loadReservations();
  }
}