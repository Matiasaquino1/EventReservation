import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { ReservationService } from '../../core/services/reservation.service'; // ajustá el path si es necesario

@Component({
  selector: 'app-my-reservations',
  standalone: true,
  imports: [CommonModule, CurrencyPipe, DatePipe],
  templateUrl: './my-reservations.component.html',
  styleUrl: './my-reservations.component.css'
})
export class MyReservationsComponent implements OnInit {
  private reservationService = inject(ReservationService);
  private router = inject(Router);

  reservations = signal<any[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  cancelling = signal<number | null>(null); // ID de la reserva que se está cancelando
  hiding = signal<number | null>(null);     // ID de la reserva que se está ocultando

  ngOnInit() {
    this.loadReservations();
  }

  loadReservations() {
    this.loading.set(true);
    this.error.set(null);

    this.reservationService.getMyReservations().subscribe({
      next: (data) => {
        this.reservations.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudieron cargar tus reservas.');
        this.loading.set(false);
      }
    });
  }

  cancelReservation(id: number) {
    if (!confirm('¿Estás seguro de que deseas cancelar esta reserva?')) return;

    this.cancelling.set(id);

    this.reservationService.cancelReservation(id).subscribe({
      next: () => {
        // Actualizar estado localmente sin recargar
        this.reservations.update(res =>
          res.map(r => r.reservationId === id ? { ...r, status: 'Cancelled' } : r)
        );
        this.cancelling.set(null);
      },
      error: () => {
        alert('No se pudo cancelar la reserva. Intentá de nuevo.');
        this.cancelling.set(null);
      }
    });
  }

  hideReservation(id: number) {
    this.hiding.set(id);

    this.reservationService.hideReservation(id).subscribe({
      next: () => {
        // Remover de la vista localmente
        this.reservations.update(res => res.filter(r => r.reservationId !== id));
        this.hiding.set(null);
      },
      error: () => {
        alert('No se pudo ocultar la reserva. Intentá de nuevo.');
        this.hiding.set(null);
      }
    });
  }

  goToPay(id: number) {
    this.router.navigate(['/payments'], { queryParams: { reservationId: id } });
  }
  
  getStatusLabel(status: string): string {
    const labels: Record<string, string> = {
      'Confirmed': '✔ Confirmada',
      'Pending': '⏳ Pendiente',
      'Cancelled': '✕ Cancelada'
    };
    return labels[status] ?? status;
  }

  getStatusClass(status: string): string {
    return 'card-' + status.toLowerCase();
  }
}