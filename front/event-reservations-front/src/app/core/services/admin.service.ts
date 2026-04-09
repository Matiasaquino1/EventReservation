import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map, Observable } from 'rxjs';

import { environment } from '../../../environments/environment.prod';
import { User } from '../models/user.model';
import { Reservation } from '../models/reservation.model';
import { DashboardStats } from '../models/admin-stats.model';

@Injectable({ providedIn: 'root' })
export class AdminService {

  private readonly apiUrl = `${environment.apiUrl}/api/Admin`;

  constructor(private http: HttpClient) {}

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

    return this.http.get<{ data: Reservation[]; page: number; pageSize: number; totalCount: number }>(
      `${this.apiUrl}/ventas`,
      { params }
    );
  }

  getUsers(page: number, limit: number): Observable<{ users: User[]; total: number }> {
    const params = new HttpParams()
    .set('page', page.toString())
    .set('limit', limit.toString());

    return this.http.get<{ users: any[], total: number }>(`${this.apiUrl}/users`, { params }).pipe(
      map(response => ({
        // Mapeamos cada usuario a nuestro modelo User, normalizando campos si es necesario
        users: response.users.map(u => this.normalizeUser(u)),
        total: response.total
      }))
    );
  }

  getDashboardStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.apiUrl}/dashboard-stats`);
  }

  getEventAttendees(eventId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/events/${eventId}/attendees`);
  }

  private normalizeUser(user: any): User {
    return {
      id: user.id,
      username: user.name || user.username || 'Sin nombre',
      email: user.email,
      role: user.role ?? 'User',
    };
}
}
