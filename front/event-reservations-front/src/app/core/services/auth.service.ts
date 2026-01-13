import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../enviroments/environment'
import { User } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    const token = localStorage.getItem('token');
    if (token) {
      // Decodificar token para obtener user (usa jwt-decode si necesitas)
      // this.currentUserSubject.next(decodedUser);
    }
  }

  login(credentials: { email: string; password: string }): Observable<any> {
    return this.http.post(`${environment.apiUrl}/Auth/login`, credentials).pipe(
      tap((res: any) => {
        localStorage.setItem('token', res.token);
        this.currentUserSubject.next(res.user);
      })
    );
  }

  register(user: { username: string; email: string; password: string }): Observable<any> {
    return this.http.post(`${environment.apiUrl}/Auth/register`, user);
  }

  logout(): void {
    localStorage.removeItem('token');
    this.currentUserSubject.next(null);
  }

  hasRole(role: string): boolean {
    const user = this.currentUserSubject.value;
    return user?.role === role;
  }

  get currentUser(): User | null {
    return this.currentUserSubject.value;
  }
}