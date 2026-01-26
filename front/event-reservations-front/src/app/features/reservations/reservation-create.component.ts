import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { ReservationService } from '../../core/services/reservation.service';
import { EventService } from '../../core/services/event.service';
import { EventModel } from '../../core/models/event.model';


@Component({
  selector: 'app-reservation-create',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reservation-create.component.html',
  styleUrls: ['./reservation-create.component.css']
})
export class ReservationCreateComponent implements OnInit {

  event: EventModel | null = null;
  error = '';
  numberOfTickets = 1;
  success = false;
  
  constructor(
    private route: ActivatedRoute,
    private eventService: EventService,
    private reservationService: ReservationService,
    private router: Router
  ) {}

  ngOnInit(): void {
  const eventId = Number(this.route.snapshot.queryParamMap.get('eventId'));

  if (!eventId) {
    this.error = 'Evento inválido';
    return;
  }

  this.eventService.getEvent(eventId).subscribe({
    next: event => {
      console.log('EVENTO:', event);
      this.event = event;
    },
    error: () => {
      this.error = 'Evento no encontrado';
    }
  });
  }

  reserve(): void {
    if (this.numberOfTickets < 1 || !this.event) return;

    this.reservationService.createReservation({
      eventId: this.event.eventId,
      numberOfTickets: this.numberOfTickets
    }).subscribe({
      next: () => {
        this.success = true;
        setTimeout(() => {
          this.router.navigate(['/my-reservations']);
        }, 1200);
      },
      error: err => {
        this.error = err.error?.message || 'Error al reservar';
      }
    });
  }
}

