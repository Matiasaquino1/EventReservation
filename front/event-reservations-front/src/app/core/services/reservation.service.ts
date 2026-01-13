import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Reservation } from '../models/reservation.model';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class ReservationService {
  createReservation(reservation: { eventId: number; numberOfTickets: number }): Observable<Reservation> {
    return this.http.post<Reservation>(`${environment.apiUrl}/Reservations`, reservation);
  }

  getMyReservations(): Observable<Reservation[]> {
    return this.http.get<Reservation[]>(
    `${environment.apiUrl}/Reservations/my`
    );
  }

  getReservations(filters?: { status?: string; eventId?: number }, page = 1, limit = 10): Observable<{ reservations: Reservation[]; total: number }> {
    let params = new HttpParams().set('page', page).set('limit', limit);
    if (filters?.status) params = params.set('status', filters.status);
    if (filters?.eventId) params = params.set('eventId', filters.eventId);
    return this.http.get<{ reservations: Reservation[]; total: number }>(`${environment.apiUrl}/Reservations`, { params });
  }

  cancelReservation(id: number): Observable<void> {
    return this.http.put<void>(`${environment.apiUrl}/Reservations/${id}/cancel`, {});
  }

  constructor(private http: HttpClient, private authService: AuthService) {}
}