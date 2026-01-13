import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { User } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class AdminService {

  private readonly apiUrl = `${environment.apiUrl}/Admin`;

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
}
