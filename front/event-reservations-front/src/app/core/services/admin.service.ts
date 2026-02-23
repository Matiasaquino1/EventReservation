import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map, Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { User } from '../models/user.model';
import { Reservation } from '../models/reservation.model';

@Injectable({ providedIn: 'root' })
export class AdminService {

  private readonly apiUrl = `${environment.apiUrl}/api/Admin`;
  private readonly usersApiUrl = `${environment.apiUrl}/api/v1/users`;
  // Alias de compatibilidad para referencias antiguas
  private readonly userApiUrl = this.usersApiUrl;

  constructor(private http: HttpClient) {}

  getUsers(page: number, limit: number): Observable<{ users: User[]; total: number }> {
    return this.http.get<any[]>(this.usersApiUrl).pipe(
      map(users => {
        const normalized = users.map(user => this.normalizeUser(user));
        const start = (page - 1) * limit;
        const pagedUsers = normalized.slice(start, start + limit);
        return {
          users: pagedUsers,
          total: normalized.length
        };
      })
    );  
  }

  promoteUser(userId: number): Observable<void> {
    return this.http.post<void>(
      `${this.apiUrl}/promote/${userId}`,
      {}
    );
  }

  deleteUser(userId: number): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/${userId}`
    );
  }

    getAdminReservations(
    page = 1,
    pageSize = 10,
    status?: string,
    eventId?: number
  ): Observable<{ data: Reservation[]; page: number; pageSize: number; totalCount: number }> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    if (status) params = params.set('status', status);
    if (eventId) params = params.set('eventId', eventId);

    return this.http.get<{ data: any[]; page: number; pageSize: number; totalCount: number }>(
      `${this.apiUrl}/reservations`,
      { params }
    ).pipe(
      map(response => ({
        ...response,
        data: response.data.map(reservation => this.normalizeAdminReservation(reservation))
      }))
    );
  }

  private NormalizeUser(user: any): User {
    return this.normalizeUser(user);
  }

  private normalizeUser(user: any): User {
    return {
      id: Number(user?.id ?? user?.userId ?? user?.UserId ?? 0),
      username: user?.username ?? user?.name ?? user?.Name ?? '',
      email: user?.email ?? user?.Email ?? '',
      role: user?.role ?? user?.Role ?? 'User'
    };
  }

  private normalizeAdminReservation(reservation: any): Reservation {
    const eventTitle = reservation?.eventTitle ?? reservation?.EventTitle;
    const userEmail = reservation?.email ?? reservation?.Email;

    return {
      reservationId: Number(reservation?.reservationId ?? reservation?.ReservationId ?? 0),
      userId: Number(reservation?.userId ?? reservation?.UserId ?? 0),
      eventId: Number(reservation?.eventId ?? reservation?.EventId ?? 0),
      status: reservation?.status ?? reservation?.Status,
      reservationDate: reservation?.reservationDate ?? reservation?.ReservationDate ?? '',
      createdAt: reservation?.createdAt ?? reservation?.CreatedAt ?? '',
      numberOfTickets: Number(reservation?.numberOfTickets ?? reservation?.NumberOfTickets ?? 0),
      user: userEmail ? { id: 0, username: userEmail, email: userEmail, role: 'User' } : undefined,
      event: eventTitle
        ? {
            eventId: Number(reservation?.eventId ?? reservation?.EventId ?? 0),
            title: eventTitle,
            location: '',
            status: 'Active',
            price: 0,
            ticketsAvailable: 0,
            totalTickets: 0,
            createdAt: ''
          }
        : undefined
    };
  }
}
