import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map, Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { User } from '../models/user.model';
import { Reservation } from '../models/reservation.model';

@Injectable({ providedIn: 'root' })
export class AdminService {

  private readonly apiUrl = `${environment.apiUrl}/api/Admin`;

  constructor(private http: HttpClient) {}

  getUsers(page: number, limit: number): Observable<{ users: User[]; total: number }> {
    return this.http.get<any[]>(this.apiUrl).pipe(
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

    return this.http.get<{ data: Reservation[]; page: number; pageSize: number; totalCount: number }>(
      `${this.apiUrl}/reservations`,
      { params }
    );
  }

  private normalizeUser(user: any): User {
    return {
      id: user.id,
      username: user.username ?? user.name ?? '',
      email: user.email,
      role: user.role ?? 'User',
    };
}
}
