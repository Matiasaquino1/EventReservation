import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { User } from '../models/user.model';
import { Reservation } from '../models/reservation.model';

@Injectable({ providedIn: 'root' })
export class AdminService {

  private readonly apiUrl = `${environment.apiUrl}/api/Admin`;

  constructor(private http: HttpClient) {}

  getUsers(page: number, limit: number): Observable<{ users: User[]; total: number }> {
    const params = new HttpParams()
      .set('page', page)
      .set('limit', limit);

    return this.http.get<{ users: User[]; total: number }>(
      `${this.apiUrl}/users`,
      { params }
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
      `${this.apiUrl}/users/${userId}`
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
}
