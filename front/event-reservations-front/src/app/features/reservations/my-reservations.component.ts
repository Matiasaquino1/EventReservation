import { Component, OnInit, inject, signal } from '@angular/core';
import { ReservationService } from '../../core/services/reservation.service';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';


@Component({
  selector: 'app-my-reservations',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './my-reservations.component.html',
  styleUrls: ['./my-reservations.component.css']
})
export class MyReservationsComponent implements OnInit {
  private reservationService = inject(ReservationService);
  private router = inject(Router);
  
  reservations = signal<any[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  ngOnInit() {
    this.loadReservations();
  }

  loadReservations() {
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
  cancel(id: number) {
    if (confirm('¿Estás seguro de que quieres cancelar esta reserva?')) {
      this.reservationService.cancelReservation(id).subscribe({
        next: () => this.loadReservations(), 
        error: () => alert('Error al cancelar')
      });
    }
  }

  goToPay(id: number) {
    this.router.navigate(['/payment'], { queryParams: { reservationId: id } });
  }

  getStatusClass(status: string) {
    return {
      'status-pending': status === 'Pending',
      'status-confirmed': status === 'Confirmed',
      'status-cancelled': status === 'Cancelled'
    };
  }
}