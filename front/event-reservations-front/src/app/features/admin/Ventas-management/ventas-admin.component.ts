import {
  Component,
  OnInit,
  inject,
  ChangeDetectorRef,
  computed,
  signal,
} from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatIconModule } from '@angular/material/icon';
import { AdminService } from '../../../core/services/admin.service';
import { ReservationService } from '../../../core/services/reservation.service';

@Component({
  selector: 'app-ventas-admin',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatPaginatorModule,
    MatIconModule,
  ],
  providers: [DatePipe, CurrencyPipe],
  templateUrl: './ventas-admin.component.html',
  styleUrls: ['./ventas-admin.component.scss'],
})
export class VentasAdminComponent implements OnInit {
  private adminService      = inject(AdminService);
  private reservationService = inject(ReservationService);
  private cdr               = inject(ChangeDetectorRef);

  reservations: any[] = [];
  statusFilter = '';
  page  = 1;
  limit = 10;
  total = 0;
  isLoading = false;

  get confirmedCount(): number {
    return this.reservations.filter(r => r.status === 'Confirmed').length;
  }
  get pendingCount(): number {
    return this.reservations.filter(r => r.status === 'Pending').length;
  }
  get cancelledCount(): number {
    return this.reservations.filter(r => r.status === 'Cancelled').length;
  }

  get totalRevenue(): number {
    return this.reservations
      .filter(r => r.status === 'Confirmed')
      .reduce((acc, r) => acc + (r.totalAmount ?? 0), 0);
  }

  ngOnInit() {
    this.loadReservations();
  }

  loadReservations() {
    this.isLoading = true;
    this.adminService
      .getAdminReservations(this.page, this.limit, this.statusFilter)
      .subscribe({
        next: (res) => {
          this.reservations = res.data;
          this.total        = res.totalCount;
          this.isLoading    = false;
          this.cdr.detectChanges();
        },
        error: () => (this.isLoading = false),
      });
  }

  onFilterChange() {
    this.page = 1; // Resetear a primera página al filtrar
    this.loadReservations();
  }

  confirmManual(reservationId: number) {
    if (!confirm('¿Confirmar pago manualmente para esta reserva?')) return;
    this.reservationService
      .forceConfirm(reservationId)
      .subscribe(() => this.loadReservations());
  }

  onPageChange(event: PageEvent) {
    this.page  = event.pageIndex + 1;
    this.limit = event.pageSize;
    this.loadReservations();
  }

  /** Etiquetas */
  statusLabel(status: string): string {
    const map: Record<string, string> = {
      Confirmed: 'Confirmado',
      Pending:   'Pendiente',
      Cancelled: 'Cancelado',
    };
    return map[status] ?? status;
  }
}