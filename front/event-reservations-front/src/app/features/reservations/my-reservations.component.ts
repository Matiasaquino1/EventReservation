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
export class MyReservationsComponent 
implements OnInit {
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

  cancelReservation(id: number) {
  if (confirm('¿Estás seguro de que deseas cancelar esta reserva?')) {
    console.log('ID que llega:', id);
    this.reservationService.cancelReservation(id).subscribe({
      next: () => {
        this.reservations.update(res => 
          res.map(r => 
            r.reservationId === id 
              ? { ...r, status: 'Cancelled' } 
              : r
          )
        );
      },
      error: () => alert('No se pudo cancelar la reserva.')
    });
    }
  }

  goToPay(id: number) {
    this.router.navigate(['/payments'], { queryParams: { reservationId: id } });
  }

  getStatusClass(status: string) {
    return {
      'status-pending': status === 'Pending',
      'status-confirmed': status === 'Confirmed',
      'status-cancelled': status === 'Cancelled'
    };
  }
}